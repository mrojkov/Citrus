#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <vorbis/ivorbisfile.h>
#include <theora/theora.h>
#include <theora/theoradec.h>

#if defined (_WIN32)
    #ifdef LEMON_EXPORTS
        #define LEMON_API __declspec(dllexport)
    #else
        #define LEMON_API __declspec(dllimport)
    #endif
#else
    #define LEMON_API
#endif
