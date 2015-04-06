using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Data;
using System.Net.Sockets;
using System.Linq;
using System.IO;

namespace TeltonikaParser
{
    /// <summary>
    /// TeltonikaFMXXXX
    /// </summary>
    public class Protocol_TeltonikaFMXXXX
    {

        private const int CODEC_FMXXX = 0x08;

        private const int ACC = 1;
        private const int DOOR = 2;
        private const int Analog = 4;
        private const int GSM = 5;
        private const int SPEED = 6;
        private const int VOLTAGE = 7;
        private const int GPSPOWER = 8;
        private const int TEMPERATURE = 9;
        private const int ODOMETER = 16;
        private const int STOP = 20;
        private const int TRIP = 28;
        private const int IMMOBILIZER = 29;
        private const int AUTHORIZED = 30;
        private const int GREEDRIVING = 31;
        private const int OVERSPEED = 33;



        private static string Parsebytes(Byte[] byteBuffer, int index, int Size)
        {
            return BitConverter.ToString(byteBuffer, index, Size).Replace("-", string.Empty);
        }

        private static string parseIMEI(Byte[] byteBuffer, int size)
        {
            int index = 0;
            var result = Parsebytes(byteBuffer, index, 2);
            return result;
        }

        private static bool checkIMEI(string data)
        {
            Console.WriteLine(data.Length);
            if (data.Length == 15)
                return true;

            return false;
        }

        private static List<Position> ParsePositions(Byte[] byteBuffer, int linesNB)
        {
            int index = 0;
            index += 7;
            uint dataSize = byteBuffer[index];

            index++;
            uint codecID = byteBuffer[index];

            if (codecID == CODEC_FMXXX)
            {
                index++;
                uint NumberOfData = byteBuffer[index];

                Console.WriteLine("{0} {1} {2} ", codecID, NumberOfData, dataSize);

                List<Position> result = new List<Position>();

                index++;
                for (int i = 0; i < NumberOfData; i++)
                {
                    Position position = new Position();

                    var timestamp = Int64.Parse(Parsebytes(byteBuffer, index, 8), System.Globalization.NumberStyles.HexNumber);
                    index += 8;

                    position.Time = DateTime.Now;

                    var Preority = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;

                    position.Lo = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    position.La = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    var Altitude = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                    index += 2;

                    var dir = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);

                    if (dir < 90) position.Direction = 1;
                    else if (dir == 90) position.Direction = 2;
                    else if (dir < 180) position.Direction = 3;
                    else if (dir == 180) position.Direction = 4;
                    else if (dir < 270) position.Direction = 5;
                    else if (dir == 270) position.Direction = 6;
                    else if (dir > 270) position.Direction = 7;
                    index += 2;

                    var Satellite = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;

                    if (Satellite >= 3)
                        position.Status = "A";
                    else
                        position.Status = "L";

                    position.Speed = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                    index += 2;

                    int ioEvent = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;
                    int ioCount = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;
                    //read 1 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;
                            //Add output status
                            switch (id)
                            {
                                case ACC:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += value == 1 ? ",ACC off" : ",ACC on";
                                        index++;
                                        break;
                                    }
                                case DOOR:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += value == 1 ? ",door close" : ",door open";
                                        index++;
                                        break;
                                    }
                                case GSM:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += string.Format(",GSM {0}", value);
                                        index++;
                                        break;
                                    }
                                case STOP:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.StopFlag = value == 1;
                                        position.IsStop = value == 1;

                                        index++;
                                        break;
                                    }
                                case IMMOBILIZER:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Alarm = value == 0 ? "Activate Anti-carjacking success" : "Emergency release success";
                                        index++;
                                        break;
                                    }
                                case GREEDRIVING:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    position.Alarm = "Acceleration intense !!";
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    position.Alarm = "Freinage brusque !!";
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    position.Alarm = "Virage serré !!";
                                                    break;
                                                }
                                            default:
                                                break;
                                        }
                                        index++;
                                        break;
                                    }
                                default:
                                    {
                                        index++;
                                        break;
                                    }
                            }

                        }
                    }

                    //read 2 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;



                            switch (id)
                            {
                                case Analog:
                                    {
                                        var value = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                                        if (value < 12)
                                            position.Alarm += string.Format("Low voltage", value);
                                        index += 2;
                                        break;
                                    }
                                case SPEED:
                                    {
                                        var value = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                                        position.Alarm += string.Format("Speed", value);
                                        index += 2;
                                        break;
                                    }
                                default:
                                    {
                                        index += 2;
                                        break;
                                    }

                            }
                        }
                    }

                    //read 4 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;

                            switch (id)
                            {
                                case TEMPERATURE:
                                    {
                                        var value = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber);
                                        position.Alarm += string.Format("Temperature {0}", value);
                                        index += 4;
                                        break;
                                    }
                                case ODOMETER:
                                    {
                                        var value = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber);
                                        position.Mileage = value;
                                        index += 4;
                                        break;
                                    }
                                default:
                                    {
                                        index += 4;
                                        break;
                                    }

                            }


                        }
                    }

                    //read 8 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;

                            var io = Int64.Parse(Parsebytes(byteBuffer, index, 8), System.Globalization.NumberStyles.HexNumber);
                            position.Status += string.Format(",{0} {1}", id, io);
                            index += 8;
                        }
                    }

                    result.Add(position);
                    Console.WriteLine(position.ToString());
                }

                return result;
            }
            return null;
        }

        public static Byte[] DealingWithHeartBeat(string data)
        {

            Byte[] result = { 1 };
            if (checkIMEI(data))
            {
                return result;
            }
            return null;
        }

        public static string ParseHeartBeatData(Byte[] byteBuffer, int size)
        {
            var IMEI = parseIMEI(byteBuffer, size);
            if (checkIMEI(IMEI))
            {
                return IMEI;
            }
            else
            {
                int index = 0;
                index += 7;
                uint dataSize = byteBuffer[index];

                index++;
                uint codecID = byteBuffer[index];

                if (codecID == CODEC_FMXXX)
                {
                    index++;
                    uint NumberOfData = byteBuffer[index];

                    return NumberOfData.ToString();
                }

            }
            return string.Empty;
        }
        public static List<Position> ParseData(Byte[] byteBuffer, int size)
        {

            List<Position> result = new List<Position>();
            result = ParsePositions(byteBuffer, size);

            return result;
        }

        public static Position GetGPRSPos(string oneLine)
        {

            return null;
        }
    }
}
