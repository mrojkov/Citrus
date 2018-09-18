#include "Lemon.h"

LEMON_API OggVorbis_File* OggCreate()
{
	return (OggVorbis_File*)malloc(sizeof(OggVorbis_File));
}

LEMON_API void OggDispose(OggVorbis_File* vf)
{
	ov_clear(vf);
	free(vf);
}

LEMON_API int OggOpen(void* dataSource, OggVorbis_File* vf, ov_callbacks callbacks)
{
	return ov_open_callbacks(dataSource, vf, NULL, 0, callbacks);
}

LEMON_API int OggRead(OggVorbis_File* vf, char* buffer, int length, int* bitstream)
{
	return ov_read(vf, buffer, length, bitstream);
}

LEMON_API void OggResetToBeginning(OggVorbis_File* vf)
{
	ov_raw_seek(vf, 0);
}

LEMON_API int OggGetFrequency(OggVorbis_File *vf)
{
	return vf->vi->rate;
}

LEMON_API int OggGetChannels(OggVorbis_File *vf)
{
 	return vf->vi->channels;
}

