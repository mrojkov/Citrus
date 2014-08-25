include $(CLEAR_VARS)

LOCAL_MODULE    := Lemon
LOCAL_PATH 		:= ../../Library/Source
LOCAL_C_INCLUDES := $(LOCAL_PATH)/Include

# To enable ASM code, uncomment following string:
# LOCAL_CFLAGS    := -D OC_ARM_ASM
# and include those files
#	Theora/armGNU/armbits.s \
#	Theora/armGNU/armfrag.s \
#	Theora/armGNU/armidct.s \
#	Theora/armGNU/armloop.s \

LOCAL_SRC_FILES := OggDecoder.c \
	OgvDecoder.c \
	TheoraDecoder.c \
	Ogg/bitwise.c \
	Ogg/framing.c \
	Theora/apiwrapper.c \
	Theora/bitpack.c \
	Theora/collect.c \
	Theora/decapiwrapper.c \
	Theora/decinfo.c \
	Theora/decode.c \
	Theora/dequant.c \
	Theora/encoder_disabled.c \
	Theora/fdct.c \
	Theora/fragment.c \
	Theora/huffdec.c \
	Theora/huffenc.c \
	Theora/idct.c \
	Theora/info.c \
	Theora/internal.c \
	Theora/mathops.c \
	Theora/quant.c \
	Theora/rate.c \
	Theora/state.c \
	Theora/tokenize.c \
	Theora/arm/armcpu.c \
	Theora/arm/armstate.c \
	Tremor/block.c \
	Tremor/codebook.c \
	Tremor/floor0.c \
	Tremor/floor1.c \
	Tremor/mapping0.c \
	Tremor/mdct.c \
	Tremor/registry.c \
	Tremor/res012.c \
	Tremor/sharedbook.c \
	Tremor/synthesis.c \
	Tremor/vorbisfile.c \
	Tremor/vorbis_info.c \
	Tremor/window.c \
	yuv2rgb/yuv2rgb16tab.c \
	yuv2rgb/yuv420rgb8888c.c \
	yuv2rgb/yuv422rgb8888c.c \
	yuv2rgb/yuv422rgb888c.c \
	yuv2rgb/yuv444rgb8888c.c

include $(BUILD_SHARED_LIBRARY)
