// MFDecoder.h

#pragma once
#include <windows.h>
#include <mfapi.h>
#include <mfidl.h>
#include <mfreadwrite.h>
#include <mferror.h>
#include <iomanip>
#include <sstream>
#include < vcclr.h >
#include <stdio.h>
#include <Propvarutil.h>
#include <comdef.h>
#include <tchar.h>


using namespace System;


namespace MFDecoder {

	template <class T> void SafeRelease(T **ppT)
	{
		if (*ppT)
		{
			(*ppT)->Release();
			*ppT = NULL;
		}
	}


	public ref class MediaFoundation abstract sealed
	{
	public:
		static bool Initialize()
		{
			HRESULT hr = MFStartup(MF_VERSION);
			if (SUCCEEDED(hr)) {
				return true;
			}
			else if (hr == MF_E_BAD_STARTUP_VERSION) {
				OutputDebugString(L"Can't initialize media encoder: MFStartup failed with MF_E_BAD_STARTUP_VERSION\n");
			}
			else if (hr == MF_E_DISABLED_IN_SAFEMODE) {
				OutputDebugString(L"Can't initialize media encoder: MFStartup failed with MF_E_DISABLED_IN_SAFEMODE\n");
			}
			else if (hr == E_NOTIMPL) {
				OutputDebugString(L"Can't initialize media encoder: MFStartup failed with E_NOTIMPL\n");
			}
			else {
				std::wstringstream ss;
				ss << L"Can't initialize media encoder: MFStartup failed with ";
				ss << L"0x" << std::hex << std::setfill(L'0') << std::setw(8) << hr << L"\n";
				OutputDebugString(ss.str().c_str());
			}
			return false;
		}

		static void Shutdown()
		{
			MFShutdown();
		}
	};
	

	public ref class Sample {
	public:
		long long PresentationTime;
		array<System::Byte>^ Data;
		bool IsAudio;
		bool IsEos;
		Sample() {
			PresentationTime = 0;
			Data = nullptr;
			IsAudio = false;
			IsEos = false;
		}
	};

	public ref class Decoder
	{
	private:
		IMFSourceReader* reader;
		int streamCount;
		bool hasVideo = false;
		bool hasAudio = false;
#ifdef _DEBUG
#define PrintResult(hr, tag) { if (SUCCEEDED(hr)) { Console::WriteLine(gcnew System::String(tag " SUCCESS {0}"), hr); } else { _com_error err(hr); Console::WriteLine(gcnew System::String(tag " Error {0}: {1}"), hr, gcnew System::String(err.ErrorMessage())); } }
#else
#define PrintResult(hr, tag) ;
#endif

		HRESULT GetDuration(LONGLONG* hnsDuration)
		{
			PROPVARIANT var;
			HRESULT hr = reader->GetPresentationAttribute(MF_SOURCE_READER_MEDIASOURCE, MF_PD_DURATION, &var);
			if (SUCCEEDED(hr)) {
				hr = PropVariantToInt64(var, hnsDuration);
				PropVariantClear(&var);
			}
			PrintResult(hr, "VideoDuration: ");
			return hr;
		}

		HRESULT EnumerateTypesForStream(DWORD streamIndex)
		{
			HRESULT hr = S_OK;
			DWORD mediaTypeIndex = 0;

			while (SUCCEEDED(hr))
			{
				IMFMediaType *mediaType = NULL;
				hr = reader->GetNativeMediaType(streamIndex, mediaTypeIndex, &mediaType);
				if (hr == MF_E_NO_MORE_TYPES)
				{
					hr = S_OK;
					break;
				}
				else if (SUCCEEDED(hr))
				{
					// Examine the media type. (Not shown.)
					mediaType->Release();
				}
				++mediaTypeIndex;
			}
			return hr;
		}

		void DebugType(IMFMediaType* type, const char* tag)
		{
			GUID subtype;
			HRESULT hr = type->GetGUID(MF_MT_SUBTYPE, &subtype);
#define check(format) if (subtype == format) { Console::WriteLine(gcnew System::String( "{0}: {1}"), gcnew System::String(tag), #format); goto checked; }
				check(MFVideoFormat_Base);
				check(MFVideoFormat_RGB32);
				check(MFVideoFormat_ARGB32);
				check(MFVideoFormat_RGB24);
				check(MFVideoFormat_RGB555);
				check(MFVideoFormat_RGB565);
				check(MFVideoFormat_RGB8);
				check(MFVideoFormat_L8);
				check(MFVideoFormat_L16);
				check(MFVideoFormat_D16);
				check(MFVideoFormat_AI44);
				check(MFVideoFormat_AYUV);
				check(MFVideoFormat_YUY2);
				check(MFVideoFormat_YVYU);
				check(MFVideoFormat_YVU9);
				check(MFVideoFormat_UYVY);
				check(MFVideoFormat_NV11);
				check(MFVideoFormat_NV12);
				check(MFVideoFormat_YV12);
				check(MFVideoFormat_I420);
				check(MFVideoFormat_IYUV);
				check(MFVideoFormat_Y210);
				check(MFVideoFormat_Y216);
				check(MFVideoFormat_Y410);
				check(MFVideoFormat_Y416);
				check(MFVideoFormat_Y41P);
				check(MFVideoFormat_Y41T);
				check(MFVideoFormat_Y42T);
				check(MFVideoFormat_P210);
				check(MFVideoFormat_P216);
				check(MFVideoFormat_P010);
				check(MFVideoFormat_P016);
				check(MFVideoFormat_v210);
				check(MFVideoFormat_v216);
				check(MFVideoFormat_v410);
				check(MFVideoFormat_MP43);
				check(MFVideoFormat_MP4S);
				check(MFVideoFormat_M4S2);
				check(MFVideoFormat_MP4V);
				check(MFVideoFormat_WMV1);
				check(MFVideoFormat_WMV2);
				check(MFVideoFormat_WMV3);
				check(MFVideoFormat_WVC1);
				check(MFVideoFormat_MSS1);
				check(MFVideoFormat_MSS2);
				check(MFVideoFormat_MPG1);
				check(MFVideoFormat_DVSL);
				check(MFVideoFormat_DVSD);
				check(MFVideoFormat_DVHD);
				check(MFVideoFormat_DV25);
				check(MFVideoFormat_DV50);
				check(MFVideoFormat_DVH1);
				check(MFVideoFormat_DVC);
				check(MFVideoFormat_H264);
				check(MFVideoFormat_H265);
				check(MFVideoFormat_MJPG);
				check(MFVideoFormat_420O);
				check(MFVideoFormat_HEVC);
				check(MFVideoFormat_HEVC_ES);
				check(MFVideoFormat_VP80);
				check(MFVideoFormat_VP90);
				check(MFVideoFormat_ORAW);

				check(MFAudioFormat_Base);
				check(MFAudioFormat_PCM);
				check(MFAudioFormat_Float);
				check(MFAudioFormat_DTS);
				check(MFAudioFormat_Dolby_AC3_SPDIF);
				check(MFAudioFormat_DRM);
				check(MFAudioFormat_WMAudioV8);
				check(MFAudioFormat_WMAudioV9);
				check(MFAudioFormat_WMAudio_Lossless);
				check(MFAudioFormat_WMASPDIF);
				check(MFAudioFormat_MSP1);
				check(MFAudioFormat_MP3);
				check(MFAudioFormat_MPEG);
				check(MFAudioFormat_AAC);
				check(MFAudioFormat_ADTS);
				check(MFAudioFormat_AMR_NB);
				check(MFAudioFormat_AMR_WB);
				check(MFAudioFormat_AMR_WP);
				check(MFAudioFormat_FLAC);
				check(MFAudioFormat_ALAC);
				check(MFAudioFormat_Opus);
#undef check
				checked:
					return;
		}

		HRESULT GetStreamType(DWORD streamIndex, GUID* streamType)
		{
			IMFMediaType* nativeType = NULL;
			HRESULT hr = reader->GetNativeMediaType(streamIndex, 0, &nativeType);
			if (FAILED(hr)) {
				return hr;
			}
			hr = nativeType->GetGUID(MF_MT_MAJOR_TYPE, streamType);
			SafeRelease(&nativeType);
			return hr;
		}

		HRESULT GetSize(DWORD streamIndex, UINT32* width, UINT32* height)
		{
			IMFMediaType* nativeType = NULL;
			HRESULT hr = reader->GetNativeMediaType(streamIndex, 0, &nativeType);
			if (FAILED(hr)) {
				return hr;
			}
			hr = MFGetAttributeSize(nativeType, MF_MT_FRAME_SIZE, width, height);
			return hr;
		}

		HRESULT GetSourceFlags(ULONG *pulFlags)
		{
			ULONG flags = 0;

			PROPVARIANT var;
			PropVariantInit(&var);

			HRESULT hr = reader->GetPresentationAttribute(
				MF_SOURCE_READER_MEDIASOURCE,
				MF_SOURCE_READER_MEDIASOURCE_CHARACTERISTICS,
				&var);
			PrintResult(hr, "GetSourceFlags: ");
			if (SUCCEEDED(hr))
			{
				hr = PropVariantToUInt32(var, &flags);
				PrintResult(hr, "PropVariantToUInt32: ");
			}
			if (SUCCEEDED(hr))
			{
				*pulFlags = flags;
			}

			PropVariantClear(&var);
			
			return hr;
		}

		HRESULT SetPositionInternal(const LONGLONG& hnsPosition)
		{
			PROPVARIANT var;
			HRESULT hr = InitPropVariantFromInt64(hnsPosition, &var);
			PrintResult(hr, "InitPropVariantFromInt64: ");
			if (SUCCEEDED(hr))
			{
				hr = reader->SetCurrentPosition(GUID_NULL, var);
				PrintResult(hr, "SetCurrentPosition: ");
				PropVariantClear(&var);
			}
			return hr;
		}

		HRESULT ConfigureStream(DWORD streamIndex)
		{
			IMFMediaType* nativeType = NULL;
			IMFMediaType* type = NULL;
			HRESULT hr = reader->GetNativeMediaType(streamIndex, 0, &nativeType);
			PrintResult(hr, "GetNativeMediaType: ");
			if (FAILED(hr)) {
				return hr;
			}
			GUID majorType;
			GUID subType;
			hr = nativeType->GetGUID(MF_MT_MAJOR_TYPE, &majorType);
			PrintResult(hr, "GetMajorType: ");
			if (FAILED(hr)) {
				goto done;
			}
			hr = MFCreateMediaType(&type);
			PrintResult(hr, "MFCreateMediaType: ");
			if (FAILED(hr)) {
				goto done;
			}
			hr = type->SetGUID(MF_MT_MAJOR_TYPE, majorType);
			PrintResult(hr, "SetMajorType: ");
			if (FAILED(hr)) {
				goto done;
			}
			if (majorType == MFMediaType_Video) {
				subType = MFVideoFormat_NV12;
			} else if (majorType == MFMediaType_Audio) {
				subType = MFAudioFormat_PCM;
			} else {
				goto done;
			}

			hr = type->SetGUID(MF_MT_SUBTYPE, subType);
			PrintResult(hr, "SetSubType: ");
			if (FAILED(hr)) {
				goto done;
			}
			hr = reader->SetCurrentMediaType(streamIndex, NULL, type);
			PrintResult(hr, "SetCurrentMediaType: ");
			if (FAILED(hr)) {
				goto done;
			}
			
		done:
			DebugType(nativeType, "NativeType");
			DebugType(type, "DesiredType");
			SafeRelease(&nativeType);
			SafeRelease(&type);
			hr = reader->GetCurrentMediaType(streamIndex, &type);
			DebugType(type, "CurrentType");
			SafeRelease(&type);
			return hr;
		}

		HRESULT ReadSample(DWORD streamIndex, IMFSample **pSample, DWORD* actualStreamIndex, DWORD* flags, LONGLONG* llTimeStamp)
		{
			static unsigned long long sampleId = 0;
			HRESULT hr = S_OK;
			
			hr = reader->ReadSample(
				MF_SOURCE_READER_ANY_STREAM,    // Stream index.
				0,                              // Flags.
				actualStreamIndex,             // Receives the actual stream index. 
				flags,                         // Receives status flags.
				llTimeStamp,                   // Receives the time stamp.
				pSample                        // Receives the sample or NULL.
			);

			if (FAILED(hr)) {
				PrintResult(hr, "ReadSample: ");
				return hr;
			}

			//wprintf(L"Stream %d (%I64d)\n", *actualStreamIndex, sampleId++);
			
			if (*flags & MF_SOURCE_READERF_ENDOFSTREAM) {
				wprintf(L"\tEnd of stream\n");
			}
			if (*flags & MF_SOURCE_READERF_NEWSTREAM) {
				wprintf(L"\tNew stream\n");
			}
			if (*flags & MF_SOURCE_READERF_NATIVEMEDIATYPECHANGED) {
				wprintf(L"\tNative type changed\n");
			}
			if (*flags & MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED) {
				wprintf(L"\tCurrent type changed\n");
			}
			if (*flags & MF_SOURCE_READERF_STREAMTICK) {
				wprintf(L"\tStream tick\n");
			}

			if (*flags & MF_SOURCE_READERF_NATIVEMEDIATYPECHANGED) {
				// The format changed. Reconfigure the decoder.
				hr = ConfigureStream(*actualStreamIndex);
				if (FAILED(hr)) {
					return hr;
				}
			}

			if (FAILED(hr)) {
				wprintf(L"ProcessSamples FAILED, hr = 0x%x\n", hr);
			}
			//fflush(stdout);
			return hr;
		}

		Sample^ ReadSample(DWORD streamIndex)
		{
			Sample^ res = gcnew Sample();
			res->Data = gcnew array<System::Byte>(0);
			IMFSample *pSample = NULL;
			DWORD actualStreamIndex, flags;
			LONGLONG llTimeStamp;
			HRESULT hr = ReadSample(streamIndex, &pSample, &actualStreamIndex, &flags, &llTimeStamp);
			if (SUCCEEDED(hr)) {
				res->PresentationTime = llTimeStamp;
				GUID sampleType;
				hr = GetStreamType(actualStreamIndex, &sampleType);
				bool isAudio = SUCCEEDED(hr) && sampleType == MFMediaType_Audio;
				res->IsAudio = isAudio;
				bool isEos = flags & MF_SOURCE_READERF_ENDOFSTREAM;
				res->IsEos = isEos;
				if (!isEos) {
					IMFMediaBuffer* buffer = NULL;
					IMF2DBuffer2* buffer2 = NULL;
					hr = pSample->ConvertToContiguousBuffer(&buffer);
					DWORD length = 0;
					BYTE* data = NULL;
					if (SUCCEEDED(hr)) {
						if (isAudio) {
							hr = buffer->Lock(&data, NULL, &length);
							if (SUCCEEDED(hr)) {
								res->Data = gcnew array<System::Byte>(length);
								System::Runtime::InteropServices::Marshal::Copy(System::IntPtr(data), res->Data, 0, length);
							}
							hr = buffer->Unlock();
						} else {
							hr = buffer->QueryInterface(IID_IMF2DBuffer2, (void**)&buffer2);
							if (SUCCEEDED(hr)) {
								BYTE* bufferStart = NULL;
								LONG stride = 0;
								DWORD bufferLength = 0;
								hr = buffer2->Lock2DSize(MF2DBuffer_LockFlags_Read, &data, &stride, &bufferStart, &bufferLength);

								if (SUCCEEDED(hr)) {
									res->Data = gcnew array<System::Byte>(bufferLength);
									System::Runtime::InteropServices::Marshal::Copy(System::IntPtr(bufferStart), res->Data, 0, bufferLength);
								}
								buffer2->Unlock2D();
							}
						}
					}
					SafeRelease(&buffer);
					SafeRelease(&buffer2);
				}
			}
			SafeRelease(&pSample);
			return res;
		}

	public:

		Decoder(String^ path)
		{
			pin_ptr<const wchar_t> wch = PtrToStringChars(path);
			pin_ptr<IMFSourceReader*> hReader = &reader;
			HRESULT hr = MFCreateSourceReaderFromURL(wch, NULL, hReader);
			if (SUCCEEDED(hr)) {
				//streamCount = -1;
				//while (SUCCEEDED(hr)) {
				//	hr = EnumerateTypesForStream(++streamCount);
				//}
				//for (int i = 0; i < streamCount; ++i) {
				//	ConfigureStream(i);
				//}
				hr = ConfigureStream(MF_SOURCE_READER_FIRST_VIDEO_STREAM);
				hasVideo = SUCCEEDED(hr);
				hr = ConfigureStream(MF_SOURCE_READER_FIRST_AUDIO_STREAM);
				hasAudio = SUCCEEDED(hr);
			}
		}

		Sample^ ReadVideoSample()
		{
			return ReadSample(MF_SOURCE_READER_FIRST_VIDEO_STREAM);
		}

		Sample^ ReadAudioSample()
		{
			return ReadSample(MF_SOURCE_READER_FIRST_VIDEO_STREAM);
		}

		Sample^ ReadSample()
		{
			return ReadSample(MF_SOURCE_READER_ANY_STREAM);
		}

		long long GetDuration()
		{
			LONGLONG res;
			HRESULT hr = GetDuration(&res);
			if (SUCCEEDED(hr)) {
				return res;
			}
			return 0;
		}

		bool SetPosition(long long p)
		{
			return SUCCEEDED(SetPositionInternal(p));
		}

		bool GetCanSeek()
		{
			BOOL bCanSeek = FALSE;
			ULONG flags;
			if (SUCCEEDED(GetSourceFlags(&flags)))
			{
				bCanSeek = ((flags & MFMEDIASOURCE_CAN_SEEK) == MFMEDIASOURCE_CAN_SEEK);
			}
			return bCanSeek;
		}

		long GetWidth()
		{
			UINT32 width;
			UINT32 height;
			HRESULT hr = GetSize(MF_SOURCE_READER_FIRST_VIDEO_STREAM, &width, &height);
			if (SUCCEEDED(hr)) {
				return width;
			}
			return 0;
		}

		long GetHeight()
		{
			UINT32 width;
			UINT32 height;
			HRESULT hr = GetSize(MF_SOURCE_READER_FIRST_VIDEO_STREAM, &width, &height);
			if (SUCCEEDED(hr)) {
				return height;
			}
			return 0;
		}
	};
}
