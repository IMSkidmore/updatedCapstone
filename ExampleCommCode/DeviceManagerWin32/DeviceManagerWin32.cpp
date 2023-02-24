// DeviceManagerWin32.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include <iostream>
#include <string>
#include <windows.h>
#include <devguid.h>    // for GUID_DEVCLASS_CDROM etc
#include <setupapi.h>
#include <cfgmgr32.h>   // for MAX_DEVICE_ID_LEN, CM_Get_Parent and CM_Get_Device_ID
#include <tchar.h>
#include <stdio.h>

#define INITGUID		// This have to be above devpkey.h
#include "devpkey.h"

using namespace std;

#define ARRAY_SIZE(arr)     (sizeof(arr)/sizeof(arr[0]))

#pragma comment (lib, "setupapi.lib")

typedef BOOL (WINAPI *FN_SetupDiGetDeviceProperty)(
  __in       HDEVINFO DeviceInfoSet,
  __in       PSP_DEVINFO_DATA DeviceInfoData,
  __in       const DEVPROPKEY *PropertyKey,
  __out      DEVPROPTYPE *PropertyType,
  __out_opt  PBYTE PropertyBuffer,
  __in       DWORD PropertyBufferSize,
  __out_opt  PDWORD RequiredSize,
  __in       DWORD Flags
);

char* convertTCHAR(TCHAR* szChar)
{
	static char pszReturnValue[1024];
	memset(pszReturnValue, 0, 1024);

	wchar_t *orig = szChar;
	size_t origsize = wcslen(orig) + 1;
	const size_t newsize = 1024;
	size_t convertedChars = 0;
	char newString[newsize];
	wcstombs_s(&convertedChars, newString, origsize, orig, _TRUNCATE);

	memcpy(pszReturnValue, newString, 1024);
	return pszReturnValue;
}

extern "C" __declspec(dllexport) void __stdcall GetUsbBusDescription(TCHAR lpVid[50], TCHAR lpPid[50], TCHAR lpComPort[50], TCHAR lpDesc[128])
{
	// First things first, check if this in Vista or higher, if not return since we can't get Bus info on XP
	OSVERSIONINFO osvi;
	memset(&osvi, 0, sizeof(OSVERSIONINFO));
    osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
    if ( (GetVersionEx(&osvi) == 0) || (osvi.dwBuildNumber < 7000) ) 
        return;
	
	string sVid;
	sVid.assign(convertTCHAR(lpVid));

	string sPid;
	sPid.assign(convertTCHAR(lpPid));

	string sComPort;
	sComPort.assign(convertTCHAR(lpComPort));

	////// Create variables
	unsigned i;
    DWORD dwSize, dwPropertyRegDataType;
    DEVPROPTYPE ulPropertyType;
    
    CONFIGRET r;
    HDEVINFO hDevInfo;
    SP_DEVINFO_DATA DeviceInfoData;
    TCHAR szDeviceInstanceID [MAX_DEVICE_ID_LEN];
    TCHAR szDesc[1024];

#ifdef UNICODE
    FN_SetupDiGetDeviceProperty fn_SetupDiGetDeviceProperty = (FN_SetupDiGetDeviceProperty)
        GetProcAddress (GetModuleHandle (TEXT("Setupapi.dll")), "SetupDiGetDevicePropertyW");
#else
    FN_SetupDiGetDeviceProperty fn_SetupDiGetDeviceProperty = (FN_SetupDiGetDeviceProperty)
        GetProcAddress(GetModuleHandle(TEXT("Setupapi.dll")), "SetupDiGetDevicePropertyA");
#endif

    // List all connected USB devices
    hDevInfo = SetupDiGetClassDevs (NULL, TEXT("USB"), NULL, DIGCF_PRESENT|DIGCF_ALLCLASSES);
    if (hDevInfo == INVALID_HANDLE_VALUE)
        return;

    // Find the ones that are driverless
    for (i = 0; ; i++)  
	{
        DeviceInfoData.cbSize = sizeof (DeviceInfoData);
        if (!SetupDiEnumDeviceInfo(hDevInfo, i, &DeviceInfoData))
            break;

        r = CM_Get_Device_ID(DeviceInfoData.DevInst, szDeviceInstanceID , MAX_PATH, 0);
        if (r != CR_SUCCESS)
            continue;

		string sDeviceId;
		sDeviceId.assign(convertTCHAR(szDeviceInstanceID));

		//_tprintf (TEXT("%s\n"), szDeviceInstanceID );
		//cout << "VID: " << sVid << "  -  PID: << sPid << \n";
		//cout << "DeviceId: " << sDeviceId << "\n\n";

		if ( (sDeviceId.find(sVid) == string::npos) || (sDeviceId.find(sPid) == string::npos) )
		{
			//_tprintf (TEXT("Found no magic!\n"));
			continue;
		}

		SetupDiGetDeviceRegistryProperty (hDevInfo, &DeviceInfoData, SPDRP_FRIENDLYNAME,
                                              &dwPropertyRegDataType, (BYTE*)szDesc,
                                              sizeof(szDesc),   // The size, in bytes
                                              &dwSize);
		
		string sFriendlyName;
		sFriendlyName.assign(convertTCHAR(szDesc));

		//_tprintf (TEXT("    Friendly Name: %s\n"), szDesc );
		//cout << "    Converted Friendly Name: " << sFriendlyName << "\n";
		//cout << "    Com Port: " << sComPort << "\n";

		if (sFriendlyName.find(sComPort) != string::npos)
		{	
			if (fn_SetupDiGetDeviceProperty && fn_SetupDiGetDeviceProperty (hDevInfo, &DeviceInfoData, &DEVPKEY_Device_BusReportedDeviceDesc,
																			&ulPropertyType, (BYTE*)szDesc, sizeof(szDesc), &dwSize, 0))
			{
				static TCHAR szReturnValue[128];
				memset(szReturnValue, 0, 128);
				memcpy(szReturnValue, szDesc, 128);

				_tcscpy_s(lpDesc, 128, szReturnValue);

				return;
			}			
		}
    }

	return;
}