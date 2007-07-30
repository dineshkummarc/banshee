using System;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Summary description for IMAPIException.
	/// </summary>
	public class IMAPIException : COMException
	{

		private readonly string imapiMessage;

		/// <summary>
		/// Wraps a <c>COMException</c> thrown by an IMAPI method with
		/// one that also includes the correct error message.
		/// </summary>
		/// <param name="innerException">Underlying <c>COMException</c></param>
		public IMAPIException(COMException innerException) : base(innerException.Message, innerException)
		{
			imapiMessage = DeriveImapiErrorMessage(innerException.ErrorCode, innerException.Message);
		}

		/// <summary>
		/// Gets the IMAPI error message
		/// </summary>
		public string IMAPIErrorMessage
		{
			get
			{
				return imapiMessage;
			}
		}

		private string DeriveImapiErrorMessage(int hResult, string defaultMessage)
		{
			string message = defaultMessage;
			switch ((uint) hResult)
			{
				case (uint) IMAPI_ERROR_CODES.IMAPI_S_PROPERTIESIGNORED:
					message = "An unknown property was passed in a property set and it was ignored.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_S_BUFFER_TOO_SMALL:
					message = "Buffer too small";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_NOTOPENED:
					message = "A call to IDiscMaster::Open has not been made.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_NOTINITIALIZED:
					message = "A recorder object has not been initialized.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_USERABORT:
					message = "The user canceled the operation.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_GENERIC:
					message = "A generic error occurred.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_MEDIUM_NOTPRESENT:
					message = "There is no disc in the device.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_MEDIUM_INVALIDTYPE:
					message = "The media is not a type that can be used.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DEVICE_NOPROPERTIES:
					message = "The recorder does not support any properties.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DEVICE_NOTACCESSIBLE:
					message = "The device cannot be used or is already in use.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DEVICE_NOTPRESENT:
					message = "The device is not present or has been removed.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DEVICE_INVALIDTYPE:
					message = "The recorder does not support an operation.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_INITIALIZE_WRITE:
					message = "The drive interface could not be initialized for writing.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_INITIALIZE_ENDWRITE:
					message = "The drive interface could not be initialized for closing.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_FILESYSTEM:
					message = "An error occurred while enabling/disabling file system access or during auto-insertion detection";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_FILEACCESS:
					message = "An error occurred while writing the image file.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DISCINFO:
					message = "An error occurred while trying to read disc data from the device.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_TRACKNOTOPEN:
					message = "An audio track is not open for writing.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_TRACKOPEN:
					message = "An open audio track is already being staged.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DISCFULL:
					message = "Disc full";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_BADJOLIETNAME:
					message = "The application tried to add a badly named element to a disc.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_INVALIDIMAGE:
					message = "The staged image is not suitable for a burn. It has been corrupted or cleared and has no usable content.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_NOACTIVEFORMAT:
					message ="An active format master has not been selected using IDiscMaster::SetActiveDiscMasterFormat.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_NOACTIVERECORDER:
					message = "An active disc recorder has not been selected using IDiscMaster::SetActiveDiscRecorder.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_WRONGFORMAT:
					message = "A call to IJolietDiscMaster has been made when IRedbookDiscMaster is the active format, or vice versa. To use a different format, change the format and clear the image file contents.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_ALREADYOPEN:
					message = "A call to IDiscMaster::Open has already been made against this object by your application.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_WRONGDISC:
					message = "The IMAPI multi-session disc has been removed from the active recorder.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_FILEEXISTS:
					message = "The file to add is already in the image file and the overwrite flag was not set.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_STASHINUSE:
					message = "Another application is already using the IMAPI stash file required to stage a disc image. Try again later.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_DEVICE_STILL_IN_USE:
					message = "Another application is already using this device, so IMAPI cannot access the device.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_LOSS_OF_STREAMING:
					message = "Content streaming was lost; a buffer under-run may have occurred.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_COMPRESSEDSTASH:
					message = "The stash is located on a compressed volume and cannot be read.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_ENCRYPTEDSTASH:
					message = "The stash is located on an encrypted volume and cannot be read.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_NOTENOUGHDISKFORSTASH:
					message = "There is not enough free space to create the stash file on the specified volume.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_REMOVABLESTASH:
					message = "The selected stash location is on a removable media.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_CANNOT_WRITE_TO_MEDIA:
					message = "Cannot write to the media.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_TRACK_NOT_BIG_ENOUGH:
					message = "Track was too short.";
					break;
				case (uint) IMAPI_ERROR_CODES.IMAPI_E_BOOTIMAGE_AND_NONBLANK_DISC:
					message = "Attempt to create a bootable image on a non-blank disc.";
					break;
			}
			return message;
		}

	}
}
