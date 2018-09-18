#include "Lemon.h"
#include "yuv2rgb/yuv2rgb.h"
#include "TheoraDecoder.h"

typedef struct
{
	int serial;
	ogg_stream_state state;
	int active;
} OgvStream;

#define MAX_STREAMS 10

typedef struct
{
	TheoraDecoder* videoDecoder;
	ogg_sync_state state;
	void* dataSource;
	ov_callbacks callbacks;
	OgvStream streams[MAX_STREAMS];
	OgvStream* videoStream;
	int streamCount;
	int fileSize;
} OgvDecoder;

int OgvReadHeaders(OgvDecoder* ogv);
int OgvReadPage(OgvDecoder* ogv, ogg_page* page);

LEMON_API OgvDecoder* OgvCreate(void* dataSource, ov_callbacks callbacks)
{
	OgvDecoder* ogv = (OgvDecoder*)malloc(sizeof(OgvDecoder));
	memset(ogv, 0, sizeof(OgvDecoder));
	ogv->callbacks = callbacks;
	ogv->videoDecoder = TheoraCreate();
	ogv->dataSource = dataSource;
	ogv->streamCount = 0;
	callbacks.seek_func(dataSource, 0, SEEK_END);
	ogv->fileSize = callbacks.tell_func(dataSource);
	callbacks.seek_func(dataSource, 0, SEEK_SET);
	ogg_sync_init(&ogv->state);
	if (OgvReadHeaders(ogv) < 0) {
		free(ogv);
		return NULL;
	}
	return ogv;
}

LEMON_API void OgvDispose(OgvDecoder* ogv)
{
	int i;
	TheoraDispose(ogv->videoDecoder);
	for (i = 0; i < ogv->streamCount; i++) {
		ogg_stream_clear(&ogv->streams[i].state);
	}
	ogg_sync_clear(&ogv->state);
	free(ogv);
}

int OgvReadHeaders(OgvDecoder* ogv)
{
	int i, ret, serial;
	ogg_page page = { 0, 0, 0, 0 };
	ogg_packet packet;
	OgvStream* stream;
	while (!ogv->videoDecoder->headerProcessed && !OgvReadPage(ogv, &page)) {
		serial = ogg_page_serialno(&page);
		if (ogg_page_bos(&page)) {
			stream = &ogv->streams[ogv->streamCount++]; 
			stream->active = 1;
			stream->serial = serial;
			ogg_stream_init(&stream->state, serial);
		}
		stream = NULL;
		for (i = 0; i < ogv->streamCount; i++) {
			if (ogv->streams[i].serial == serial) {
				stream = &ogv->streams[i];
			}
		}
		if (stream == NULL) {
			return -1;
		}
		// Add a complete page to the bitstream
		ret = ogg_stream_pagein(&stream->state, &page);
		if (ret < 0) {
			return ret;
		}
		// Process all available header packets in the stream. When we hit
		// the first data stream we don't decode it, instead we
		// return. The caller can then choose to process whatever data
		// streams it wants to deal with.
		memset(&packet, 0, sizeof(packet));
		while (!ogv->videoDecoder->headerProcessed && (ret = ogg_stream_packetpeek(&stream->state, &packet)) != 0) {
			if (ret != 1) {
				return -1;
			}
			if (!ogv->videoStream || serial == ogv->videoStream->serial) {
				// A packet is available. If it is not a header packet we exit.
				// If it is a header packet, process it as normal.
				ret = TheoraHandleHeader(ogv->videoDecoder, &packet);
				if (ret == 0) {
					ogv->videoStream = stream;
				} else if (ret < 0) {
					return -1;
				}
			}
			if (!ogv->videoDecoder->headerProcessed) {
				// Consume the packet
				ret = ogg_stream_packetout(&stream->state, &packet);
				if (ret != 1) {
					return -1;
				}
			}
		}
	}
	if (ogv->videoDecoder->headerProcessed) {
		if (TheoraInitialize(ogv->videoDecoder) < 0) {
			return -1;
		}
	}
	return 0;
}

int OgvReadPage(OgvDecoder* ogv, ogg_page* page) 
{
	int bytes;
	int ret = 0;
	
	// If we've hit end of file we still need to continue processing
	// any remaining pages that we've got buffered.
	if (ogv->callbacks.tell_func(ogv->dataSource) == ogv->fileSize) {
		return ogg_sync_pageout(&ogv->state, page) == 1 ? 0 : -1;
	}
	while ((ret = ogg_sync_pageout(&ogv->state, page)) != 1) {
		// Returns a buffer that can be written too
		// with the given size. This buffer is stored
			// in the ogg synchronization structure.
		char* buffer = ogg_sync_buffer(&ogv->state, 4096);
		if (buffer == NULL) {
			return -1;
		}
		// Read from the file into the buffer
		bytes = ogv->callbacks.read_func(buffer, 4096, 1, ogv->dataSource);
		if (bytes == 0) {
			// End of file. 
			continue;
		}
		// Update the synchronization layer with the number
		// of bytes written to the buffer
		ret = ogg_sync_wrote(&ogv->state, bytes);
		if (ret != 0) {
			return -1;
		}
	}
	return 0;
}

int OgvReadPacket(OgvDecoder* ogv, OgvStream* stream, ogg_packet* packet)
{
	ogg_page page = { 0, 0, 0, 0 };
	OgvStream* pageStream;
	int serial, i, ret;
	while ((ret = ogg_stream_packetout(&stream->state, packet)) != 1) {
		ret = OgvReadPage(ogv, &page);
		if (ret < 0) {
			return -1;
		}
		serial = ogg_page_serialno(&page);
		pageStream = NULL;
		for (i = 0; i < ogv->streamCount; i++) {
			if (ogv->streams[i].serial == serial) {
				pageStream = &ogv->streams[i];
			}
		}
		if (pageStream == NULL) {
			return -1;
		}
		ret = ogg_stream_pagein(&pageStream->state, &page);
		if (ret < 0) {
			return -1;
		}
	}
	return 0;
}

LEMON_API int OgvDecodeFrame(OgvDecoder* ogv)
{
	// Decode one frame and display it. If no frame is available we
	// don't do anything.
	ogg_packet packet;
	memset(&packet, 0, sizeof(packet));
	if (!OgvReadPacket(ogv, ogv->videoStream, &packet)) {
		if (TheoraHandlePacket(ogv->videoDecoder, &packet) < 0) {
			return -1;
		}
		return 0;
	}
	return -1;
}

LEMON_API int OgvGetVideoWidth(OgvDecoder* ogv)
{
	return ogv->videoDecoder->info.frame_width;
}

LEMON_API int OgvGetVideoHeight(OgvDecoder* ogv)
{
	return ogv->videoDecoder->info.frame_height;
}

LEMON_API th_img_plane OgvGetBuffer(OgvDecoder* ogv, int plane)
{
	return ogv->videoDecoder->buffer[plane];
}

LEMON_API double OgvGetPlaybackTime(OgvDecoder* ogv)
{
	double time = th_granule_time(ogv->videoDecoder->ctx, ogv->videoDecoder->granulepos);
	return time;
}

LEMON_API void DecodeRGBX8(uint8_t *dst_ptr,
    const uint8_t  *y_ptr,
    const uint8_t  *u_ptr,
    const uint8_t  *v_ptr,
    int32_t   width,
    int32_t   height,
    int32_t   y_span,
    int32_t   uv_span,
    int32_t   dst_span,
    int32_t   dither)
{
	yuv420_2_rgb8888(dst_ptr, y_ptr, u_ptr, v_ptr, width, height,
		y_span, uv_span, dst_span, yuv2rgb565_table, dither);
}