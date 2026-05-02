/*
 * Copyright (c) 2025, NVIDIA CORPORATION.  All rights reserved.
 *
 * NVIDIA CORPORATION and its licensors retain all intellectual property
 * and proprietary rights in and to this software, related documentation
 * and any modifications thereto.  Any use, reproduction, disclosure or
 * distribution of this software and related documentation without an express
 * license agreement from NVIDIA CORPORATION is strictly prohibited.
 */

/*
 *   █████  █████ ██████ ████  ████   ███████   ████  ██████ ██   ██
 *   ██  ██ ██      ██   ██    ██  ██ ██ ██ ██ ██  ██   ██   ██   ██
 *   ██  ██ ██      ██   ██    ██  ██ ██ ██ ██ ██  ██   ██   ██   ██
 *   ██████ ████    ██   ████  █████  ██ ██ ██ ██████   ██   ███████
 *   ██  ██ ██      ██   ██    ██  ██ ██    ██ ██  ██   ██   ██   ██
 *   ██  ██ ██      ██   █████ ██  ██ ██    ██ ██  ██   ██   ██   ██   DEBUGGER
 *                                                           ██   ██
 *  ████████████████████████████████████████████████████████ ██ █ ██ ████████████
 *
 *
 *  HOW TO EDIT AFTERMATH GPU CRASH DUMPS:
 *  ---------------------------------------
 *
 *  1)  Call 'GFSDK_Aftermath_GpuCrashDump_CreateEditor', to create an editor
 *      object from a GPU crash dump.
 *
 *
 *  2) Call one or more of the 'GFSDK_Aftermath_GpuCrashDumpEditor_*' functions with this
 *     editor, to modify the GPU crash dump data:
 *
 *     - 'GFSDK_Aftermath_GpuCrashDumpEditor_ResolveEventMarkers' to resolve event marker data
 *     - 'GFSDK_Aftermath_GpuCrashDumpEditor_AddDescription' to add description key-value pairs
 *
 *     These functions will modify the crash dump data held by the editor object.
 *
 *
 *  3) Call 'GFSDK_Aftermath_GpuCrashDumpEditor_GetCrashDumpData' to obtain the modified
 *     crash dump data from the editor.
 *
 *
 *  4) Call 'GFSDK_Aftermath_GpuCrashDump_DestroyEditor', to destroy the editor object
 *     and cleanup all related memory.
 *
 */

#ifndef GFSDK_Aftermath_GpuCrashDumpEditing_H
#define GFSDK_Aftermath_GpuCrashDumpEditing_H

#include "GFSDK_Aftermath_Defines.h"
#include "GFSDK_Aftermath_GpuCrashDump.h"

#pragma pack(push, 8)

#ifdef __cplusplus
extern "C" {
#endif

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDump_Editor
// ---------------------------------
//
// GPU crash dump editor handle.
//
// This type represents a modifiable GPU crash dump. After creation, it allows
// the modification of various fields within the crash dump.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_AFTERMATH_DECLARE_HANDLE(GFSDK_Aftermath_GpuCrashDump_Editor);

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDump_CreateEditor
// ---------------------------------
//
// apiVersion;
//     Must be set to GFSDK_Aftermath_Version_API. Used for checking against
//     library version.
//
// pGpuCrashDump;
//     Pointer to GPU crash dump data captured in a 'GFSDK_Aftermath_GpuCrashDumpCb'
//     callback.
//
// gpuCrashDumpSize;
//     Size of GPU crash dump data in bytes.
//
// pEditor;
//     Pointer to receive the created editor handle.
//     On success, the caller owns the handle and must destroy it using
//     'GFSDK_Aftermath_GpuCrashDump_DestroyEditor'.
//
//// DESCRIPTION;
//     Create a GPU crash dump editor object from raw crash dump data.
//     The editor will have its own independent copy of the crash dump data.
//     The editor object is owned by the caller and must be destroyed using
//     'GFSDK_Aftermath_GpuCrashDump_DestroyEditor'.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_API GFSDK_Aftermath_GpuCrashDump_CreateEditor(
    GFSDK_Aftermath_Version apiVersion,
    const void* pGpuCrashDump,
    const uint32_t gpuCrashDumpSize,
    GFSDK_Aftermath_GpuCrashDump_Editor* pEditor);

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDump_DestroyEditor
// ---------------------------------
//
// editor;
//     The editor handle to be destroyed. After this call returns, the handle
//     becomes invalid and must not be used.
//
//// DESCRIPTION;
//     Destroys a GPU crash dump editor object and frees its resources.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_API GFSDK_Aftermath_GpuCrashDump_DestroyEditor(
    const GFSDK_Aftermath_GpuCrashDump_Editor editor);

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDumpEditor_ResolveEventMarkers
// ---------------------------------
//
// editor;
//     A valid editor handle.
//
// resolveMarkerCb;
//     Callback function to resolve event marker data.
//     Called once for every event marker in the crash dump.
//
// pUserData;
//     User data passed to the callback.
//
//// DESCRIPTION;
//     Allows for manipulating event marker data using additional application knowledge
//     not available at crash dump generation time. This function will modify the crash
//     dump data held by the editor.
//
// NOTE: For resolved markers, the pointer value is preserved for zero-sized payloads.
// For non-zero-sized payloads, the data is fully preserved (when resolved via the callback),
// but the stored pointer value may refer to an internal buffer. This consistent behavior
// applies during post-generation resolution.
//
// NOTE: During driver-side crash dump generation, individual event marker payloads may be
// truncated to 1024 bytes. Post-generation editing via this API does not impose an additional
// size limit; the payload provided via 'resolveMarker' is embedded into the edited crash dump.
// To avoid truncation at generation time, prefer application-managed markers by setting
// 'markerDataSize = 0' in 'GFSDK_Aftermath_SetEventMarker'.
//
// Threading requirement: The provided 'resolveMarker' functor must be invoked from the same
// thread and before the callback returns.
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_API GFSDK_Aftermath_GpuCrashDumpEditor_ResolveEventMarkers(
    const GFSDK_Aftermath_GpuCrashDump_Editor editor,
    PFN_GFSDK_Aftermath_ResolveMarkerCb resolveMarkerCb,
    void* pUserData);

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDumpEditor_AddDescription
// ---------------------------------
//
// editor;
//     A valid editor handle.
//
// key;
//     Key must be one of the predefined keys of 'GFSDK_Aftermath_GpuCrashDumpDescriptionKey'
//     or a user-defined key based on 'GFSDK_Aftermath_GpuCrashDumpDescriptionKey_UserDefined'.
//     All keys greater than the last predefined key in 'GFSDK_Aftermath_GpuCrashDumpDescriptionKey'
//     and smaller than 'GFSDK_Aftermath_GpuCrashDumpDescriptionKey_UserDefined' are considered
//     illegal; attempting to add such a key will not add the description and will return
//     'GFSDK_Aftermath_Result_FAIL_InvalidParameter'.
//
// value;
//     Data to be attached to the key.
//
//// DESCRIPTION;
//     Allows for adding description key-value pairs to a crash dump.
//     This function will modify the crash dump data held by the editor.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_API GFSDK_Aftermath_GpuCrashDumpEditor_AddDescription(
    const GFSDK_Aftermath_GpuCrashDump_Editor editor,
    uint32_t key,
    const char* value);

/////////////////////////////////////////////////////////////////////////
// GFSDK_Aftermath_GpuCrashDumpEditor_GetCrashDumpData
// ---------------------------------
//
// editor;
//     A valid editor handle.
//
// pBuffer;
//     Caller allocated buffer to be populated with the crash dump data.
//     Can be nullptr to query the required buffer size.
//
// bufferSize;
//     The size in bytes of the caller allocated buffer where to store
//     the crash dump data. Must be 0 if pBuffer is not provided.
//
// pCrashDumpSize;
//     The size in bytes of the crash dump data.
//
//// DESCRIPTION;
//     Copies the crash dump data associated with the editor object
//     into a caller provided buffer. This includes all modifications
//     made to the crash dump via the editor API.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_API GFSDK_Aftermath_GpuCrashDumpEditor_GetCrashDumpData(
    const GFSDK_Aftermath_GpuCrashDump_Editor editor,
    void* pBuffer,
    const uint32_t bufferSize,
    uint32_t* pCrashDumpSize);

/////////////////////////////////////////////////////////////////////////
//
// Function pointer definitions - if dynamic loading is preferred.
//
/////////////////////////////////////////////////////////////////////////
GFSDK_Aftermath_PFN(GFSDK_AFTERMATH_CALL* PFN_GFSDK_Aftermath_GpuCrashDump_CreateEditor)(GFSDK_Aftermath_Version apiVersion, const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize, GFSDK_Aftermath_GpuCrashDump_Editor* pEditor);
GFSDK_Aftermath_PFN(GFSDK_AFTERMATH_CALL* PFN_GFSDK_Aftermath_GpuCrashDump_DestroyEditor)(const GFSDK_Aftermath_GpuCrashDump_Editor editor);
GFSDK_Aftermath_PFN(GFSDK_AFTERMATH_CALL* PFN_GFSDK_Aftermath_GpuCrashDumpEditor_ResolveEventMarkers)(const GFSDK_Aftermath_GpuCrashDump_Editor editor, PFN_GFSDK_Aftermath_ResolveMarkerCb resolveMarkerCb, void* pUserData);
GFSDK_Aftermath_PFN(GFSDK_AFTERMATH_CALL* PFN_GFSDK_Aftermath_GpuCrashDumpEditor_AddDescription)(const GFSDK_Aftermath_GpuCrashDump_Editor editor, uint32_t key, const char* value);
GFSDK_Aftermath_PFN(GFSDK_AFTERMATH_CALL* PFN_GFSDK_Aftermath_GpuCrashDumpEditor_GetCrashDumpData)(const GFSDK_Aftermath_GpuCrashDump_Editor editor, void* pBuffer, const uint32_t bufferSize, uint32_t* pCrashDumpSize);

#ifdef __cplusplus
} // extern "C"
#endif

#pragma pack(pop)

#endif // GFSDK_Aftermath_GpuCrashDumpEditing_H
