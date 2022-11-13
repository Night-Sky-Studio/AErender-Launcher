using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AErenderLauncher.Classes.Project {
    public class ProjectItem {
        public string Name { get; set; } = "";
        public int[] FootageDimensions { get; set; } = new int[2];
        public double FootageFramerate { get; set; }
        public uint[] Frames { get; set; } = new uint[2];
        public byte[] BackgroundColor { get; set; } = new byte[3];

        public override string ToString() {
            return $"{Name, -36} | [{FootageDimensions[0], 6},{FootageDimensions[1], 6}] | {FootageFramerate, -8} | [{Frames[0], 8},{Frames[1], 8}] | [{BackgroundColor[0], 3},{BackgroundColor[1], 3},{BackgroundColor[2], 3}]";
        }
    }
    
    public struct UInt24 {
        public readonly byte _b0, _b1, _b2;
        public UInt24(UInt32 val) {
            _b0 = (byte)((val >> 16) & 0xFF);
            _b1 = (byte)((val >> 8) & 0xFF);
            _b2 = (byte)(val & 0xFF);
        }

        public UInt24(byte[] val) {
            _b0 = val[0];
            _b1 = val[1];
            _b2 = val[2];
        }
        
        public UInt32 ToUInt32() {
            return (uint)_b0 << 16 | (uint)_b1 << 8 | _b2;
        }

        public byte[] ToByteArray() {
            return new[] { _b0, _b1, _b2 };
        }

        public override string ToString() {
            return ToUInt32().ToString();
        }

        public static implicit operator UInt32(UInt24 u) {
            return u.ToUInt32();
        }

        public static implicit operator byte[](UInt24 u) {
            return u.ToByteArray();
        }

        public static implicit operator UInt24(byte[] u) {
            return new UInt24(u);
        }
        
        public static implicit operator UInt24(UInt32 u) {
            return new UInt24(u);
        }

        public UInt32 Value => ToUInt32();
    }
    
    public static class Aeparser {
        // Search for a string s in binary stream and return it's position
        public static long PositionOf(this Stream stream, string s, Encoding encoding, long offset = 0) {
            byte[] bytes = encoding.GetBytes(s);
            byte[] buffer = new byte[bytes.Length];
            stream.Seek(offset, SeekOrigin.Begin);
            while (stream.Position < stream.Length - 3 && stream.Read(buffer, 0, buffer.Length) > 0) {
                if (bytes.SequenceEqual(buffer)) {
                    return stream.Position;
                }
                stream.Seek(-buffer.Length + 1, SeekOrigin.Current);
            }
            return -1;
        }
        
        /// <summary>
        /// Parses binary RIFX AE project file by looking CDTA chunks
        /// </summary>
        /// <param name="ProjectPath">Project path</param>
        /// <returns>null if an error occured</returns>
        public static List<ProjectItem>? ParseProject(string ProjectPath) {
            List<ProjectItem> result = new List<ProjectItem>();
            try {
                // hovno
                //FileStream stream = new FileStream(ProjectPath, FileMode.Open, FileAccess.Read);
                
                // much faster
                // read file to byte array
                
                byte[] bytes = File.ReadAllBytes(ProjectPath);
                // convert byte array to stream
                MemoryStream stream = new MemoryStream(bytes);
                
                // check if we have RIFX header
                //long startSeek = stream.Position;
                BinaryReader rd = new(stream);
                //uint riffSize;

                string riffMagic = Encoding.UTF8.GetString(rd.ReadBytes(4));
                if (riffMagic != "RIFX") throw new Exception("This is not a RIFF file");

                // move
                rd.ReadUInt32();
                string riffFormat = Encoding.UTF8.GetString(rd.ReadBytes(4));
                if (riffFormat != "Egg!") throw new Exception("This is not an AEP file");

                long seekpos = 0;

                while (seekpos < stream.Length) {
                    // find item data
                    seekpos = stream.PositionOf("idta", Encoding.UTF8, seekpos);

                    if (seekpos == -1) break;

                    // skip until size
                    // 96 - from idta offset
                    stream.Seek(seekpos + 92, SeekOrigin.Begin);

                    int name_size = BitConverter.ToInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);
                    string name = Encoding.UTF8.GetString(rd.ReadBytes(name_size));
                    
                    // right after name there is LIST
                    seekpos = stream.PositionOf("LIST", Encoding.UTF8, stream.Position);
                    
                    if (seekpos == -1) break;
                    
                    stream.Seek(seekpos, SeekOrigin.Begin);
                    // LIST size
                    int list_size = BitConverter.ToInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);
                    stream.Seek(list_size, SeekOrigin.Current);
                    
                    // now, we get to cdta
                    string cdta = Encoding.UTF8.GetString(rd.ReadBytes(4));
                    if (cdta == "cdta") {
                        //Console.Out.WriteLine($"Comp name: {name, -36}");
                        // cdta size
                        stream.Seek(4, SeekOrigin.Current);
                        //int cdta_size = BitConverter.ToInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);
                        
                        // skip 4 bytes
                        stream.Seek(4, SeekOrigin.Current);
                        
                        // framerate divisor
                        int framerate_divisor = BitConverter.ToInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);
                        // framerate dividend
                        int framerate_dividend = BitConverter.ToInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);

                        // skip 6 bytes
                        stream.Seek(6, SeekOrigin.Current);
                        
                        // playhead position 02 58 [00 29 3C] 00 00 00 00
                        stream.Seek(2, SeekOrigin.Current);
                        stream.Seek(3, SeekOrigin.Current);
                        //UInt24 playhead_position = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                        stream.Seek(3, SeekOrigin.Current);
                        
                        // start frame 78 00 [00 23 10] 00 00 00
                        stream.Seek(2, SeekOrigin.Current);
                        UInt24 start_frame = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                        stream.Seek(3, SeekOrigin.Current);
                        
                        // end frame 78 00 [00 24 9C] 00 00 00
                        stream.Seek(2, SeekOrigin.Current);
                        UInt24 end_frame = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                        UInt24 end_frame_end_offset = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                        
                        // duration 78 00 [01 19 40] 00 00 00
                        stream.Seek(2, SeekOrigin.Current);
                        UInt24 duration = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                        stream.Seek(3, SeekOrigin.Current);
                        
                        // background color 78 00 [00 00 00]
                        stream.Seek(2, SeekOrigin.Current);
                        UInt24 background_color = rd.ReadBytes(3);
                        
                        // big skip here
                        stream.Seek(85, SeekOrigin.Current);
                        
                        // width [07 80] 04 38
                        int width = BitConverter.ToInt16(rd.ReadBytes(2).ReverseForBigEndian(0, 2), 0);
                        // height 07 80 [04 38]
                        int height = BitConverter.ToInt16(rd.ReadBytes(2).ReverseForBigEndian(0, 2), 0);
                        
                        // skip 12 bytes - empty
                        stream.Seek(12, SeekOrigin.Current);
                        
                        // rounded framerate, needed later
                        int framerate = BitConverter.ToInt16(rd.ReadBytes(2).ReverseForBigEndian(0, 2), 0);

                        // 6 bytes - unknown
                        stream.Seek(6, SeekOrigin.Current);
                        // start offset
                        uint start_offset = BitConverter.ToUInt32(rd.ReadBytes(4).ReverseForBigEndian(0, 4), 0);
                        stream.Seek(2, SeekOrigin.Current);
                        
                        // a tricky one
                        // if comparison_framerate == framerate, then use start_offset as it
                        // else divide start_offset by 2
                        int comparison_framerate = BitConverter.ToInt16(rd.ReadBytes(2).ReverseForBigEndian(0, 2), 0);

                        if (comparison_framerate != framerate) start_offset /= 2;

                        uint[] frames;

                        if (end_frame._b0 > 0x13 && end_frame._b1 > 0xC6 && end_frame._b2 > 0x80 || end_frame > 0x0013C680|| end_frame_end_offset._b0 > 0x00) {
                            frames = new[] {
                                (start_offset + start_frame) / 2,
                                (start_offset + duration) / 2
                            };
                        } else {
                            frames = new[] {
                                (start_offset + start_frame) / 2,
                                (start_offset + end_frame) / 2,
                            };
                        }
                        
                        // combine all together
                        result.Add(new () {
                            FootageDimensions = new[] { width, height },
                            FootageFramerate = (double)framerate_dividend / framerate_divisor,
                            Frames = frames,
                            Name = name,
                            BackgroundColor = background_color,
                        });
                    }
                }
                return result;
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
                return null;
            }
        }

        public static async Task<List<ProjectItem>?> ParseProjectAsync(string ProjectPath) {
            return await Task.Run(() => ParseProject(ProjectPath));
        }
    }
}