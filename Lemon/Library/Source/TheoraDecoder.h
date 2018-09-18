typedef struct
{
	th_info info;
	th_comment comment;
	th_setup_info* setup;
	th_dec_ctx* ctx;
	th_ycbcr_buffer buffer;
	ogg_int64_t granulepos;
	int headerProcessed;
} TheoraDecoder;

TheoraDecoder* TheoraCreate();
int TheoraInitialize(TheoraDecoder* theora);
void TheoraDispose(TheoraDecoder* theora);
int TheoraHandlePacket(TheoraDecoder* theora, ogg_packet* packet);
int TheoraHandleHeader(TheoraDecoder* theora, ogg_packet* packet);