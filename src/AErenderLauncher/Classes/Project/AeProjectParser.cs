using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AErenderLauncher.Classes.Extensions;

namespace AErenderLauncher.Classes.Project;

public static class AeProjectParser {
    // Search for a string s in binary stream and return its position
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
    /// <param name="projectPath">Project path</param>
    /// <returns>null if an error occured</returns>
    public static List<ProjectItem>? ParseProject(string projectPath) {
        List<ProjectItem> result = new List<ProjectItem>();
        try {
            // hovno
            //FileStream stream = new FileStream(ProjectPath, FileMode.Open, FileAccess.Read);

            // much faster
            // read file to byte array

            byte[] bytes = File.ReadAllBytes(projectPath);
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

                int nameSize = BitConverter.ToInt32(rd.ReadBytes(4)
                    .ReverseForBigEndian(0, 4), 0);
                string name = Encoding.UTF8.GetString(rd.ReadBytes(nameSize));

                // right after name there is LIST
                seekpos = stream.PositionOf("LIST", Encoding.UTF8, stream.Position);

                if (seekpos == -1) break;

                stream.Seek(seekpos, SeekOrigin.Begin);
                // LIST size
                int listSize = BitConverter.ToInt32(rd.ReadBytes(4)
                    .ReverseForBigEndian(0, 4), 0);
                stream.Seek(listSize, SeekOrigin.Current);

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
                    int framerateDivisor = BitConverter.ToInt32(rd.ReadBytes(4)
                        .ReverseForBigEndian(0, 4), 0);
                    // framerate dividend
                    int framerateDividend = BitConverter.ToInt32(rd.ReadBytes(4)
                        .ReverseForBigEndian(0, 4), 0);

                    // skip 6 bytes
                    stream.Seek(6, SeekOrigin.Current);

                    // playhead position 02 58 [00 29 3C] 00 00 00 00
                    stream.Seek(2, SeekOrigin.Current);
                    stream.Seek(3, SeekOrigin.Current);
                    //UInt24 playhead_position = rd.ReadBytes(3);//.ReverseForBigEndian(0, 3);
                    stream.Seek(3, SeekOrigin.Current);

                    // start frame 78 00 [00 23 10] 00 00 00
                    stream.Seek(2, SeekOrigin.Current);
                    UInt24 startFrame = rd.ReadBytes(3); //.ReverseForBigEndian(0, 3);
                    stream.Seek(3, SeekOrigin.Current);

                    // end frame 78 00 [00 24 9C] 00 00 00
                    stream.Seek(2, SeekOrigin.Current);
                    UInt24 endFrame = rd.ReadBytes(3); //.ReverseForBigEndian(0, 3);
                    UInt24 endFrameEndOffset = rd.ReadBytes(3); //.ReverseForBigEndian(0, 3);

                    // duration 78 00 [01 19 40] 00 00 00
                    stream.Seek(2, SeekOrigin.Current);
                    UInt24 duration = rd.ReadBytes(3); //.ReverseForBigEndian(0, 3);
                    stream.Seek(3, SeekOrigin.Current);

                    // background color 78 00 [00 00 00]
                    stream.Seek(2, SeekOrigin.Current);
                    UInt24 backgroundColor = rd.ReadBytes(3);

                    // big skip here
                    stream.Seek(85, SeekOrigin.Current);

                    // width [07 80] 04 38
                    int width = BitConverter.ToInt16(rd.ReadBytes(2)
                        .ReverseForBigEndian(0, 2), 0);
                    // height 07 80 [04 38]
                    int height = BitConverter.ToInt16(rd.ReadBytes(2)
                        .ReverseForBigEndian(0, 2), 0);

                    // skip 12 bytes - empty
                    stream.Seek(12, SeekOrigin.Current);

                    // rounded framerate, needed later
                    int framerate = BitConverter.ToInt16(rd.ReadBytes(2)
                        .ReverseForBigEndian(0, 2), 0);

                    // 6 bytes - unknown
                    stream.Seek(6, SeekOrigin.Current);
                    // start offset
                    uint startOffset = BitConverter.ToUInt32(rd.ReadBytes(4)
                        .ReverseForBigEndian(0, 4), 0);
                    stream.Seek(2, SeekOrigin.Current);

                    // a tricky one
                    // if comparison_framerate == framerate, then use start_offset as it
                    // else divide start_offset by 2
                    int comparisonFramerate = BitConverter.ToInt16(rd.ReadBytes(2)
                        .ReverseForBigEndian(0, 2), 0);

                    if (comparisonFramerate != framerate) startOffset /= 2;

                    uint[] frames;

                    if (endFrame._b0 > 0x13 && endFrame._b1 > 0xC6 && endFrame._b2 > 0x80 ||
                        endFrame > 0x0013C680 || endFrameEndOffset._b0 > 0x00) {
                        frames = new[] {
                            (startOffset + startFrame) / 2,
                            (startOffset + duration) / 2
                        };
                    } else {
                        frames = new[] {
                            (startOffset + startFrame) / 2,
                            (startOffset + endFrame) / 2,
                        };
                    }

                    // combine all together
                    result.Add(new() {
                        FootageDimensions = new[] { width, height },
                        FootageFramerate = (double)framerateDividend / framerateDivisor,
                        Frames = frames,
                        Name = name,
                        BackgroundColor = backgroundColor,
                    });
                }
            }

            return result;
        } catch (Exception e) {
            Console.Error.WriteLine(e.ToString());
            return null;
        }
    }

    public static async Task<List<ProjectItem>?> ParseProjectAsync(string projectPath)
        => await Task.Run(() => ParseProject(projectPath));
}