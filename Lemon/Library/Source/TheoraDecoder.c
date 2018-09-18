#include "Lemon.h"
#include "TheoraDecoder.h"

TheoraDecoder* TheoraCreate()
{
	TheoraDecoder* theora = (TheoraDecoder*)malloc(sizeof(TheoraDecoder));
	memset(theora, 0, sizeof(TheoraDecoder));
	theora->ctx = 0;
	theora->setup = 0;
	theora->granulepos = 0;
	theora->headerProcessed = 0;
	memset(theora->buffer, 0, sizeof(theora->buffer));
	th_info_init(&theora->info);
	th_comment_init(&theora->comment);
	return theora;
}

void TheoraDispose(TheoraDecoder* theora)
{
	th_info_clear(&theora->info);
	th_comment_clear(&theora->comment);
	if(theora->setup)
		th_setup_free(theora->setup);
	if (theora->ctx)
		th_decode_free(theora->ctx);
	free(theora);
}

int TheoraInitialize(TheoraDecoder* theora)
{
	int ret;
	int ppmax = 0;
	theora->ctx = th_decode_alloc(&theora->info, theora->setup);
	if (theora->ctx == NULL) {
		return -1;
	}
	ret = th_decode_ctl(theora->ctx, TH_DECCTL_GET_PPLEVEL_MAX, &ppmax, sizeof(ppmax));
	if (ret != 0) {
		return -1;
	}
	// Set to a value between 0 and ppmax inclusive to experiment with
	// this parameter.
	ppmax = 0;
	ret = th_decode_ctl(theora->ctx, TH_DECCTL_SET_PPLEVEL, &ppmax, sizeof(ppmax));
	if (ret != 0) {
		return -1;
	}
	return 0;
}

int TheoraHandlePacket(TheoraDecoder* theora, ogg_packet* packet) 
{
	// The granulepos for a packet gives the time of the end of the
	// display interval of the frame in the packet.  We keep the
	// granulepos of the frame we've decoded and use this to know the
	// time when to display the next frame.
	int ret = th_decode_packetin(theora->ctx, packet, &theora->granulepos);
	if (ret && ret != TH_DUPFRAME) 
		return -1;

	// If the return code is TH_DUPFRAME then we don't need to
	// get the YUV data and display it since it's the same as
	// the previous frame.

	// We have a frame. Get the YUV data
	ret = th_decode_ycbcr_out(theora->ctx, theora->buffer);
	if (ret != 0)
		return -1;
	return 0;
}

int TheoraHandleHeader(TheoraDecoder* theora, ogg_packet* packet)
{
	int ret = th_decode_headerin(&theora->info, &theora->comment, &theora->setup, packet);
	if (ret == TH_ENOTFORMAT) {
		// Not a theora header
		return 1;
	}

	if (ret > 0) {
		// This is a theora header packet
		return 0;
	}

	// Any other return value is treated as a fatal error
	if (ret != 0)
		return -1;

	// This is not a header packet. It is the first
	// video data packet.
	theora->headerProcessed = 1;
	return 1;
}