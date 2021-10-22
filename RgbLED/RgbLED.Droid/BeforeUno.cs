
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace BeforeUno
{

	namespace Windows.Storage.Streams
	{

		public partial interface IBuffer
		{
			uint Capacity
			{
				get;
			}

			uint Length
			{
				get;
				set;
			}

			internal byte[] Data { get; }
		}
		// ale to juz jest w Uno, dodaję jeden construct
		public class InMemoryBuffer : IBuffer
		{
			internal byte[] Data { get; }
			internal InMemoryBuffer(int capacity)
			{
				Data = new byte[capacity];
			}

			internal InMemoryBuffer(byte[] data)
			{
				Data = data;
			}

			byte[] IBuffer.Data => Data;


			public uint Capacity => (uint)Data.Length;

			public uint Length
			{
				get => (uint)Data.Length;
				set => throw new NotSupportedException();
			}
		}

		public enum ByteOrder
		{
			LittleEndian,
			BigEndian,
		}

		public enum UnicodeEncoding
		{
			Utf8,
			Utf16LE,
			Utf16BE
		}

		public enum InputStreamOptions
		{
			None,
			Partial,
			ReadAhead,
		}

		public partial interface IDataReader
		{
			ByteOrder ByteOrder { get; set; }
			InputStreamOptions InputStreamOptions { get; set; }
			uint UnconsumedBufferLength { get; }
			UnicodeEncoding UnicodeEncoding { get; set; }
			byte ReadByte();
			void ReadBytes(byte[] value);
			IBuffer ReadBuffer(uint length);
			bool ReadBoolean();
			Guid ReadGuid();
			short ReadInt16();
			int ReadInt32();
			long ReadInt64();
			ushort ReadUInt16();
			uint ReadUInt32();
			ulong ReadUInt64();
			float ReadSingle();
			double ReadDouble();
			string ReadString(uint codeUnitCount);
			DateTimeOffset ReadDateTime();
			TimeSpan ReadTimeSpan();
			DataReaderLoadOperation LoadAsync(uint count);
			IBuffer DetachBuffer();
			IInputStream DetachStream();
		}

		public partial class DataReader : IDataReader, IDisposable
		{
			private IBuffer _buffer = null;
			private uint _position = 0;

			public ByteOrder ByteOrder { get; set; }

			public uint UnconsumedBufferLength
			{
				get => _buffer.Length - _position;
			}
			//public DataReader(global::Windows.Storage.Streams.IInputStream inputStream)
			//{
			//	global::Windows.Foundation.Metadata.ApiInformation.TryRaiseNotImplemented("Windows.Storage.Streams.DataReader", "DataReader.DataReader(IInputStream inputStream)");
			//}

			private void CheckPosition(int size)
			{
				if (_buffer.Length >= (_position + size))
				{
					return;
				}

				throw new IndexOutOfRangeException("Windows.Storage.Streams.DataReader - reading past EOF");
			}

			public byte ReadByte()
			{
				CheckPosition(1);
				byte value = _buffer.Data[_position];
				_position++;
				return value;
			}

			#region "required by interface"
			// methods required by interface, but still unimplemented
			public UnicodeEncoding UnicodeEncoding { get; set; }
			public InputStreamOptions InputStreamOptions { get; set; }

			[global::Uno.NotImplemented]
			public void ReadBytes(byte[] value)
			{
				throw new global::System.NotImplementedException("Windows.Storage.Streams.DataReader.ReadBytes(byte[]) is not implemented in Uno.");
			}

			[global::Uno.NotImplemented]
			public string ReadString(uint codeUnitCount)
			{
				throw new global::System.NotImplementedException("The member string DataReader.ReadString(uint codeUnitCount) is not implemented in Uno.");
			}

			[global::Uno.NotImplemented]
			public DataReaderLoadOperation LoadAsync(uint count)
			{
				throw new global::System.NotImplementedException("The member DataReaderLoadOperation DataReader.LoadAsync(uint count) is not implemented in Uno.");
			}

			[global::Uno.NotImplemented]
			public IInputStream DetachStream()
			{
				throw new global::System.NotImplementedException("The member IInputStream DataReader.DetachStream() is not implemented in Uno.");
			}

			#endregion

			public IBuffer ReadBuffer(uint length)
			{
				CheckPosition((int)length);

				var buffer = new InMemoryBuffer((int)length);
				for (int i = 0; i < length; i++)
				{
					buffer.Data[i] = _buffer.Data[_position];
					_position++;
				}
				return buffer;
			}
			public bool ReadBoolean()
			{
				byte value = ReadByte();
				if (value == 0)
				{
					return false;
				}
				return true;        // although DataWriter uses 0/1, we can also interpret all non-zero values as true;
			}

			private byte[] ReadBytes(int size)
			{
				CheckPosition(size);

				byte[] buffer = new byte[size];
				for (int i = 0; i < size; i++)
				{
					buffer[i] = _buffer.Data[_position];
					_position++;
				}

				// maybe we should reverse array (if platform endianess is different than requested)
				if ((ByteOrder == ByteOrder.LittleEndian && !BitConverter.IsLittleEndian)
					|| (ByteOrder == ByteOrder.BigEndian && BitConverter.IsLittleEndian))
				{
					Array.Reverse(buffer);
				}

				return buffer;
			}

			public Guid ReadGuid()
			{
				Int32 u32 = ReadInt32();
				Int16 u16a = ReadInt16();
				Int16 u16b = ReadInt16();
				byte[] u64 = ReadBytes(8);

				var value = new Guid(u32, u16a, u16b, u64);
				return value;
			}
			public short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);
			public int ReadInt32() => BitConverter.ToInt32(ReadBytes(4), 0);
			public long ReadInt64() => BitConverter.ToInt64(ReadBytes(8), 0);
			public ushort ReadUInt16() => BitConverter.ToUInt16(ReadBytes(2), 0);
			public uint ReadUInt32() => BitConverter.ToUInt32(ReadBytes(4), 0);
			public ulong ReadUInt64() => BitConverter.ToUInt64(ReadBytes(8), 0);
			public float ReadSingle()
			{// yes, it is endianness dependant
				return BitConverter.ToSingle(ReadBytes(4), 0);
			}
			public double ReadDouble()
			{// yes, it is endianness dependant
				return BitConverter.ToDouble(ReadBytes(8), 0);
			}
			public DateTimeOffset ReadDateTime()
			{
				long ticks = ReadInt64();
				var date = new DateTime(1601, 1, 1, 0, 0, 0).ToLocalTime();
				date = date.AddTicks(ticks);

				return date;
			}
			public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks(ReadInt64());
			public IBuffer DetachBuffer() => _buffer;
			public void Dispose()
			{
				// for DataReader(IBuffer), nothing to do;
			}

			internal DataReader(IBuffer buffer)
			{
				_buffer = buffer;
			}

			public static DataReader FromBuffer(IBuffer buffer) => new DataReader(buffer);
		}
			public partial interface IDataWriter
			{
				ByteOrder ByteOrder
				{
					get;
					set;
				}
				uint UnstoredBufferLength
				{
					get;
				}
				UnicodeEncoding UnicodeEncoding
				{
					get;
					set;
				}

				void WriteByte(byte value);
				void WriteBytes(byte[] value);
				void WriteBuffer(IBuffer buffer);
				void WriteBuffer(IBuffer buffer, uint start, uint count);
				void WriteBoolean(bool value);
				void WriteGuid(global::System.Guid value);
				void WriteInt16(short value);
				void WriteInt32(int value);
				void WriteInt64(long value);
				void WriteUInt16(ushort value);
				void WriteUInt32(uint value);
				void WriteUInt64(ulong value);
				void WriteSingle(float value);
				void WriteDouble(double value);
				void WriteDateTime(DateTimeOffset value);
				void WriteTimeSpan(TimeSpan value);
				uint WriteString(string value);
				uint MeasureString(string value);
				DataWriterStoreOperation StoreAsync();
				IAsyncOperation<bool> FlushAsync();
				IBuffer DetachBuffer();
				IOutputStream DetachStream();
			}

			public partial class DataWriter : IDataWriter, IDisposable
			{
				// only one will be not null - depends on constructor used
				private System.IO.MemoryStream _memStream = null;
				private IOutputStream _outputStream = null;
				// z: fileOutputStream = detachStream();
				// OutputStreamOverStream.cs , bez konstruktora, jako io.stream

				private static int _unicodeOrder = 0;

				public UnicodeEncoding UnicodeEncoding { get; set; }

				public ByteOrder ByteOrder { get; set; }

				public uint UnstoredBufferLength
				{
					get => (uint)(_memStream.Length - _memStream.Position);
				}

				#region "required by interface"
				// methods required by interface, but still unimplemented

				[global::Uno.NotImplemented]
				public DataWriterStoreOperation StoreAsync()
				{
					// when constructor was without paramaters:
					// System.Runtime.InteropServices.COMException: 'The operation identifier is not valid.
					throw new InvalidOperationException("Windows.Storage.Streams.DataWriter.StoreAsync called, but no Stream was provided in Constructor");
					// else: should do something ...
				}
				#endregion

				public IAsyncOperation<bool> FlushAsync() => FlushAsyncTask().AsAsyncOperation<bool>();

				private async System.Threading.Tasks.Task<bool> FlushAsyncTask()
				{
					if (_memStream != null)
					{
						await _memStream.FlushAsync();
						return true;
					}
					return true;
					//if(_outputStream != null)
					//{
					//	return await _outputStream.FlushAsync();
					//}
				}

				IOutputStream IDataWriter.DetachStream()
				{
					// only when constructor was without IOutputStream
					return null;
				}


				public DataWriter()
				{
					_memStream = new System.IO.MemoryStream();
				}

				public void WriteByte(byte value)
				{
					_memStream.WriteByte(value);
				}

				public void WriteBytes(byte[] value)
				{
					_memStream.Write(value, 0, value.Length);
				}

				public void WriteBoolean(bool value)
				{
					// same as System.BitConverter, but without call to it
					if (value)
						_memStream.WriteByte(1);
					else
						_memStream.WriteByte(0);
				}

				public void WriteGuid(Guid value)
				{
					// GUID is written as int32, int16, int16, int64; endianness is per every integer part, not for whole GUID
					byte[] guidBytes = value.ToByteArray();
					WriteInt32(BitConverter.ToInt32(guidBytes, 0));
					WriteInt16(BitConverter.ToInt16(guidBytes, 4));
					WriteInt16(BitConverter.ToInt16(guidBytes, 6));
					WriteInt64(BitConverter.ToInt64(guidBytes, 8));
				}

				private void WriteBytes(byte[] bytes, int size)
				{
					// maybe we should reverse array (if platform endianess is different than requested)
					if ((ByteOrder == ByteOrder.LittleEndian && !BitConverter.IsLittleEndian)
						|| (ByteOrder == ByteOrder.BigEndian && BitConverter.IsLittleEndian))
					{
						Array.Reverse(bytes);
					}

					_memStream.Write(bytes, 0, size);
				}

				public void WriteInt16(short value) => WriteBytes(BitConverter.GetBytes(value), 2);
				public void WriteInt32(int value) => WriteBytes(BitConverter.GetBytes(value), 4);
				public void WriteInt64(long value) => WriteBytes(BitConverter.GetBytes(value), 8);
				public void WriteUInt16(ushort value) => WriteBytes(BitConverter.GetBytes(value), 2);
				public void WriteUInt32(uint value) => WriteBytes(BitConverter.GetBytes(value), 4);
				public void WriteUInt64(ulong value) => WriteBytes(BitConverter.GetBytes(value), 8);
				public void WriteSingle(float value)
				{   // yes, it is endianness dependant
					WriteBytes(BitConverter.GetBytes(value), 4);
				}
				public void WriteDouble(double value)
				{   // yes, it is endianness dependant
					WriteBytes(BitConverter.GetBytes(value), 8);
				}
				public void WriteDateTime(DateTimeOffset value)
				{ // it seems like documentation error...
				  // implementation here is based on testing how it works on UWP
					var epoch1600 = new DateTime(1601, 1, 1, 0, 0, 0);
					long ticks = (value - epoch1600.ToLocalTime()).Ticks;
					WriteInt64(ticks);
				}
				public void WriteTimeSpan(TimeSpan value) => WriteInt64(value.Ticks);

				public uint MeasureString(string value)
				{
					switch (this.UnicodeEncoding)
					{
						case UnicodeEncoding.Utf16BE:
							return (uint)(System.Text.Encoding.Unicode.GetBytes(value).Length);
						case UnicodeEncoding.Utf16LE:
							return (uint)(System.Text.Encoding.BigEndianUnicode.GetBytes(value).Length);
						default: // UnicodeEncoding.Utf8
							return (uint)(System.Text.Encoding.UTF8.GetBytes(value).Length);
					}
				}

				public uint WriteString(string value)
				{
					byte[] dump;

					switch (this.UnicodeEncoding)
					{
						case UnicodeEncoding.Utf16BE:
							dump = System.Text.Encoding.Unicode.GetBytes(value);
							break;
						case UnicodeEncoding.Utf16LE:
							dump = System.Text.Encoding.BigEndianUnicode.GetBytes(value);
							break;
						default: // UnicodeEncoding.Utf8
							dump = System.Text.Encoding.UTF8.GetBytes(value);
							break;
					}
					WriteBytes(dump);
					return (uint)dump.Length;
				}

				public void WriteBuffer(IBuffer buffer)
				{
					WriteBytes(buffer.Data);
				}

				public void WriteBuffer(IBuffer buffer, uint start, uint count)
				{
					byte[] partBuffer = new byte[count];
					for (int i = 0; i < count; i++)
					{
						partBuffer[i] = buffer.Data[start + i];
					}
					WriteBytes(partBuffer);
				}


				public IBuffer DetachBuffer()
				{
					byte[] array = _memStream.ToArray();    // makes copy, and truncate on length (GetBuffer doesn't make copy, but also doesn't truncate)
					var buffer = new InMemoryBuffer(array);
					return buffer;
				}

				public void Dispose()
				{
					_memStream?.Dispose();
					_outputStream?.Dispose();
				}


			}

		public class TestDataWriterReader
		{
			public static void testCase()
			{
				bool saveBool = true;
				byte saveByte = 10;
				int saveInt = 12345;
				double saveDouble = 12345.678;
				DateTimeOffset saveDate = DateTimeOffset.Now;
				TimeSpan saveTimeSpan = TimeSpan.FromMinutes(10);
				Guid saveGuid = Guid.NewGuid();

				var writer = new DataWriter();
				writer.WriteBoolean(saveBool);
				writer.WriteByte(saveByte);
				writer.WriteInt32(saveInt);
				writer.WriteDouble(saveDouble);
				writer.WriteDateTime(saveDate);
				writer.WriteTimeSpan(saveTimeSpan);
				writer.WriteGuid(saveGuid);

				var buffer = writer.DetachBuffer();

				var reader = DataReader.FromBuffer(buffer);

				bool loadBool = reader.ReadBoolean();
				byte loadByte = reader.ReadByte();
				int loadInt = reader.ReadInt32();
				double loadDouble = reader.ReadDouble();
				DateTimeOffset loadDate = reader.ReadDateTime();
				TimeSpan loadTimeSpan = reader.ReadTimeSpan();
				Guid loadGuid = reader.ReadGuid();


			}
		}

	}

	namespace Windows.Extensions
    {
		public partial class PermissionsHelper
        {
			/// <summary>
			/// Checks if the given Android permission is declared in manifest file.
			/// </summary>
			/// <param name="permission">Permission.</param>
			/// <returns></returns>
			public static bool IsDeclaredInManifest(string permission)
			{
				var context = Android.App.Application.Context;
				var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, Android.Content.PM.PackageInfoFlags.Permissions);
				var requestedPermissions = packageInfo?.RequestedPermissions;

				return requestedPermissions?.Any(r => r.Equals(permission, StringComparison.OrdinalIgnoreCase)) ?? false;
			}

		}

	}

	namespace Windows.Devices.Radios
	{

		#region "feat: Windows.Devices.Radios common structs/enums #5589"

		public partial class Radio
		{
			public RadioKind Kind { get; internal set; }

			public string Name { get; internal set; }

			public RadioState State { get; internal set; }

		}
		public enum RadioAccessStatus
		{
			Unspecified,
			Allowed,
			DeniedByUser,
			DeniedBySystem,
		}

		public enum RadioKind
		{
			Other,
			WiFi,
			MobileBroadband,
			Bluetooth,
			FM,
		}

		public enum RadioState
		{
			Unknown,
			On,
			Off,
			Disabled,
		}
		#endregion

		#region "feat: GetRadiosAsync()"
		// using System.Threading.Tasks;
		public partial class Radio
		{

			private static Radio GetRadiosBluetooth()
			{
				Android.Content.Context context = Android.App.Application.Context;
				bool btFull = context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetooth);
				bool btLE = context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
				if (!(btFull || btLE))
				{
					return null;
				}

				var oRadio = new Radio();
				oRadio.Kind = RadioKind.Bluetooth;
				oRadio.Name = "Bluetooth";  // name as in UWP

				if (!Windows.Extensions.PermissionsHelper.IsDeclaredInManifest(Android.Manifest.Permission.Bluetooth))
				{
					oRadio.State = RadioState.Unknown;
					return oRadio;
				}

				if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.JellyBeanMr1)
				{
					var btAdapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
					if (btAdapter == null)
					{
						return null;    // shouldn't happen...
					}
					else if (!btAdapter.IsEnabled)
					{
						oRadio.State = RadioState.Off;
					}
					else
					{
						oRadio.State = RadioState.On;
					}

					return oRadio;
				}
				else
				{
					var btAdapter = ((Android.Bluetooth.BluetoothManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.BluetoothService)).Adapter;
					if (btAdapter == null)
					{
						return null;    // shouldn't happen...
					}
					if (btAdapter.State != Android.Bluetooth.State.On)
					{
						oRadio.State = RadioState.Off;
					}
					else
					{
						oRadio.State = RadioState.On;
					}

					return oRadio;
				}

			}

			private static Radio GetRadiosWiFi()
			{
				Android.Content.Context context = Android.App.Application.Context;
				if (!context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureWifi))
				{
					return null;
				}

				var oRadio = new Radio();
				oRadio.Kind = RadioKind.WiFi;
				oRadio.Name = "Wi-Fi";  // name as in UWP

				if (!Windows.Extensions.PermissionsHelper.IsDeclaredInManifest(Android.Manifest.Permission.AccessNetworkState))
				{
					oRadio.State = RadioState.Unknown;
					return oRadio;
				}

				var connManager = (Android.Net.ConnectivityManager)context.GetSystemService(Android.Content.Context.ConnectivityService);

				if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.LollipopMr1)
				{
					// since API 23
					var activeNetwork = connManager.ActiveNetwork;
					if (activeNetwork is null)
					{
						return null;
					}
                    var netCaps = connManager.GetNetworkCapabilities(activeNetwork);
                    if (netCaps is null)
                    {
                        return null;
                    }
                    if (netCaps.HasTransport(Android.Net.TransportType.Wifi))
					{
						oRadio.State = RadioState.On;
					}
					else
					{
						oRadio.State = RadioState.Off;
					}

					return oRadio;
				}
				else
				{
					// for Android API 1 to 28 (deprecated in 29)
					var netInfo = connManager.ActiveNetworkInfo;
					if (netInfo is null)
					{
						oRadio.State = RadioState.Off;
					}
					else
					{
#pragma warning disable CS0618 // Type or member is obsolete
						if (netInfo.Type == Android.Net.ConnectivityType.Wifi ||
							(netInfo.Type == Android.Net.ConnectivityType.Wimax))
#pragma warning restore CS0618 // Type or member is obsolete
						{
							oRadio.State = RadioState.On;
						}
						else
						{
							oRadio.State = RadioState.Off;
						}
					}
					return oRadio;
				}


			}

			private static Radio GetRadiosMobile()
			{
				Android.Content.Context context = Android.App.Application.Context;
				if (!context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureTelephony))
				{
					return null;
				}

				var oRadio = new Radio();
				oRadio.Kind = RadioKind.MobileBroadband;
				oRadio.Name = "Cellular";  // I have "Cellular 6" (Lumia 532), but maybe "6" doesn't mean anything

				if (!Windows.Extensions.PermissionsHelper.IsDeclaredInManifest(Android.Manifest.Permission.AccessNetworkState))
				{
					oRadio.State = RadioState.Unknown;
					return oRadio;
				}

				var connManager = (Android.Net.ConnectivityManager)context.GetSystemService(Android.Content.Context.ConnectivityService);

				if (Android.OS.Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.P)
				{
					// available since API 23
					var activeNetwork = connManager.ActiveNetwork;
					if (activeNetwork is null)
					{
						return null;
					}
					var netCaps = connManager.GetNetworkCapabilities(activeNetwork);
					if (netCaps is null)
					{
						return null;
					}
					if (netCaps.HasTransport(Android.Net.TransportType.Cellular))
					{
						oRadio.State = RadioState.On;
					}
					else
					{
						oRadio.State = RadioState.Off;
					}

					return oRadio;
				}
				else
				{
					// for Android API 1 to 28 (deprecated in 29)
					var netInfo = connManager.ActiveNetworkInfo;
					if (netInfo is null)
					{
						oRadio.State = RadioState.Off;
					}
					else
					{
#pragma warning disable CS0618 // Type or member is obsolete
						if (netInfo.Type == Android.Net.ConnectivityType.Mobile)
#pragma warning restore CS0618 // Type or member is obsolete
						{
							oRadio.State = RadioState.On;
						}
						else
						{
							oRadio.State = RadioState.Off;
						}
					}
					return oRadio;
				}


			}


			private async static Task<System.Collections.Generic.IReadOnlyList<Radio>> GetRadiosAsyncTask()
			{// this method can be not Async/await, but I don't know how to convert it to IAsyncOperation

				var oRadios = new System.Collections.Generic.List<Radio>();

				var oRadio = GetRadiosBluetooth();
				if (oRadio != null) oRadios.Add(oRadio); // yield oRadio;

				oRadio = GetRadiosWiFi();
				if (oRadio != null) oRadios.Add(oRadio); // yield oRadio;

				oRadio = GetRadiosMobile();
				if (oRadio != null) oRadios.Add(oRadio); // yield oRadio;

				return oRadios;
			}
			
			/// <summary>
			/// Gets info about the phones for a contact.
			/// </summary>
			public static IAsyncOperation<System.Collections.Generic.IReadOnlyList<Radio>> GetRadiosAsync()
			{
				return GetRadiosAsyncTask().AsAsyncOperation();
			}

			#endregion
		}
	}

}