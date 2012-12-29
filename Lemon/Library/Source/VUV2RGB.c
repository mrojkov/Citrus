#include "Lemon.h"

#define FRAC_BITS 13
	
#define CLAMP_BYTE(x) ((x & 0xFFFFFF00) == 0 ? x : (x & 0x80000000 ? 0 : 255))
	
#define DECODE_YUV(Y, rV, gUV, bU)			\
	CLAMP_BYTE((Y + bU ) >> FRAC_BITS) << 16 |		\
	CLAMP_BYTE((Y - gUV) >> FRAC_BITS) << 8 |		\
	CLAMP_BYTE((Y + rV ) >> FRAC_BITS) |			\
	0xFF000000
	
#define DECODE_YUVA(Y, rV, gUV, bU, a)		\
	CLAMP_BYTE((Y + bU ) >> FRAC_BITS) << 16 |		\
	CLAMP_BYTE((Y - gUV) >> FRAC_BITS) << 8 |		\
	CLAMP_BYTE((Y + rV ) >> FRAC_BITS) |			\
	a << 24
	
int YTable [256];
int BUTable[256];
int GUTable[256];
int GVTable[256];
int RVTable[256];
	
int tablesReady = 0;

void InitTables()
{
	int i;
	double scale = 1 << FRAC_BITS;
	for (i = 0; i < 256; i++)
	{
		YTable[i]  = (int)((1.164 * scale + 0.5) * (i - 16));
		RVTable[i] = (int)((1.596 * scale + 0.5) * (i - 128));
		GUTable[i] = (int)((0.391 * scale + 0.5) * (i - 128));
		GVTable[i] = (int)((0.813 * scale + 0.5) * (i - 128));
		BUTable[i] = (int)((2.018 * scale + 0.5) * (i - 128));
	}
}
	
LEMON_API void DecodeRGBX(char* yData, char* uData, char* vData, 
	int yWidth, int yHeight, int yStride,
	int uvWidth, int uvHeight, int uvStride,
	unsigned int* rgbx, int stride)
{
	char cu, cv;
	int x, y, rV, gUV, bU, Y;
	char* ySrc, *uSrc, *vSrc;
	unsigned int* dst1, *dst2;
	
	ySrc = yData;
	uSrc = uData;
	vSrc = vData;
	dst1 = rgbx;
	dst2 = rgbx + stride / 4;

	if (!tablesReady) {
		tablesReady = 1;
		InitTables();
	}
		
	for (y = 0; y < yHeight; y += 2) {
		for (x = 0; x < yWidth; x += 2) {
			cu = *uSrc++;
			cv = *vSrc++;
			rV  = RVTable[cv];
			gUV = GUTable[cu] + GVTable[cv];
			bU  = BUTable[cu];
				
			Y = YTable[*ySrc];
			*dst1++ = DECODE_YUV(Y, rV, gUV, bU);
				
			Y = YTable[*(ySrc + 1)];
			*dst1++ = DECODE_YUV(Y, rV, gUV, bU);
				
			Y = YTable[*(ySrc + yStride)];
			*dst2++ = DECODE_YUV(Y, rV, gUV, bU);
				
			Y = YTable[*(ySrc + yStride + 1)];
			*dst2++ = DECODE_YUV(Y, rV, gUV, bU);
				
			ySrc += 2;
		}
		dst1 += stride / 2 - yWidth;
		dst2 += stride / 2 - yWidth;
		ySrc += yStride * 2 - yWidth;
		uSrc += uvStride - uvWidth;
		vSrc += uvStride - uvWidth;
	}
}
	