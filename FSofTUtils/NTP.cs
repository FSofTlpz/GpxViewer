using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FSofTUtils {
   public class NTP {

      /* https://de.wikipedia.org/wiki/Network_Time_Protocol
       * 
       * Die Zeitstempel im NTP sind 64 Bits lang. 
       * 32 Bits kodieren die Sekunden seit dem 1. Januar 1900, 00:00:00 Uhr, weitere 32 Bits den Sekundenbruchteil. 
       * Auf diese Weise lässt sich ein Zeitraum von 232 Sekunden (etwa 136 Jahre) mit einer Auflösung von 2−32 Sekunden (etwa 0,23 Nanosekunden) darstellen. 

https://datatracker.ietf.org/doc/html/rfc5905

   0                   1                   2                   3
   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |LI | VN  |Mode |    Stratum     |     Poll      |  Precision   |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                         Root Delay                            |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                         Root Dispersion                       |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                          Reference ID                         |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   +                     Reference Timestamp (64)                  +
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   +                      Origin Timestamp (64)                    +
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   +                      Receive Timestamp (64)                   +
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   +                      Transmit Timestamp (64)                  +    4x4 + 4x8 = 48 Byte
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

   .                    Extension Field 1 (variable)               .
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   .                    Extension Field 2 (variable)               .
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                          Key Identifier                       |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                            dgst (128)                         |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

   Leap Indicator (LI, 2Bit)
      Code warning of impending leap-second to be inserted at the end of the last day of the current month. Bits are coded as follows:
           +-------+----------------------------------------+
           | Value | Meaning                                |
           +-------+----------------------------------------+
           | 0     | no warning                             |
           | 1     | last minute of the day has 61 seconds  |
           | 2     | last minute of the day has 59 seconds  |
           | 3     | unknown (clock unsynchronized)         |
           +-------+----------------------------------------+
   
   version number (VN)
         3-bit integer representing the NTP version number, currently 4.

   Protocol Modes
         +-------+--------------------------+
         | Value | Meaning                  |
         +-------+--------------------------+
         | 0     | reserved                 |
         | 1     | symmetric active         |
         | 2     | symmetric passive        |
         | 3     | client                   |
         | 4     | server                   |
         | 5     | broadcast                |
         | 6     | NTP control message      |
         | 7     | reserved for private use |
         +-------+--------------------------+

   Reference Clock Type (8 Bit)
   (Type)
      Code identifying the type of reference clock. Values are defined
      as follows:
         0       unspecified
         1       primary reference (e.g. radio clock)
         2       secondary reference using an Internet host via NTP
         3       secondary reference using some other host or protocol
         4       eyeball-and-wristwatch
         (additional codes reserved for future use)

   Stratum (stratum): 
      8-bit integer representing the stratum
        +--------+-----------------------------------------------------+
        | Value  | Meaning                                             |
        +--------+-----------------------------------------------------+
        | 0      | unspecified or invalid                              |
        | 1      | primary server (e.g., equipped with a GPS receiver) |
        | 2-15   | secondary server (via NTP)                          |
        | 16     | unsynchronized                                      |
        | 17-255 | reserved                                            |
        +--------+-----------------------------------------------------+

   Poll
      8-bit signed integer representing the maximum interval between successive messages, in log2 seconds.  Suggested default limits for
      minimum and maximum poll intervals are 6 and 10, respectively.

   Precision
      8-bit signed integer representing the precision of the system clock, in log2 seconds.  For instance, a value of -18
      corresponds to a precision of about one microsecond. The precision can be determined when the service first starts up as the minimum
      time of several iterations to read the system clock.

   Root Delay (rootdelay): Total round-trip delay to the reference clock, in NTP short format.

   Root Dispersion (rootdisp): Total dispersion to the reference clock, in NTP short format.

   Reference ID (refid): 
      32-bit code identifying the particular server or reference clock.
     +------+----------------------------------------------------------+
     | ID   | Clock Source                                             |
     +------+----------------------------------------------------------+
     | GOES | Geosynchronous Orbit Environment Satellite               |
     | GPS  | Global Position System                                   |
     | GAL  | Galileo Positioning System                               |
     | PPS  | Generic pulse-per-second                                 |
     | IRIG | Inter-Range Instrumentation Group                        |
     | WWVB | LF Radio WWVB Ft. Collins, CO 60 kHz                     |
     | DCF  | LF Radio DCF77 Mainflingen, DE 77.5 kHz                  |
     | HBG  | LF Radio HBG Prangins, HB 75 kHz                         |
     | MSF  | LF Radio MSF Anthorn, UK 60 kHz                          |
     | JJY  | LF Radio JJY Fukushima, JP 40 kHz, Saga, JP 60 kHz       |
     | LORC | MF Radio LORAN C station, 100 kHz                        |
     | TDF  | MF Radio Allouis, FR 162 kHz                             |
     | CHU  | HF Radio CHU Ottawa, Ontario                             |
     | WWV  | HF Radio WWV Ft. Collins, CO                             |
     | WWVH | HF Radio WWVH Kauai, HI                                  |
     | NIST | NIST telephone modem                                     |
     | ACTS | NIST telephone modem                                     |
     | USNO | USNO telephone modem                                     |
     | PTB  | European telephone modem                                 |
     +------+----------------------------------------------------------+

   Reference Timestamp
      Time when the system clock was last set or corrected, in NTP timestamp format.

   Origin Timestamp (org)
      Time at the client when the request departed for the server, in NTP timestamp format.

   Receive Timestamp (rec)
      Time at the server when the request arrived from the client, in NTP timestamp format.

   Transmit Timestamp (xmt)
      Time at the server when the response left for the client, in NTP timestamp format.

   Destination Timestamp (dst)
      Time at the client when the reply arrived from the server, in NTP timestamp format.


   The destination peer calculates the roundtrip delay and clock offset relative to the source peer as follows. Let t1, t2 and t3
   represent the contents of the Originate Timestamp, Receive Timestamp and Transmit Timestamp fields and t4 the local time the
   NTP message is received.  Then the roundtrip delay d and clock offset c is:

      d = (t4 - t1) - (t3 - t2)  
      c = (t2 - t1 + t3 - t4) / 2

   The implicit assumption in the above is that the one-way delay is statistically half the roundtrip delay and that the intrinsic
   drift rates of both the client and server clocks are small and close to the same value.

       */

      /* Windows:
z.B.
w32tm /query /status

   Sprungindikator: 0(keine Warnung)
   Stratum: 4 (Sekundärreferenz - synchr. über (S)NTP)
   Präzision: -23 (119.209ns pro Tick)
   Stammverzögerung: 0.0054700s
   Stammabweichung: 0.2811642s
   Referenz-ID: 0x0A7D0179 (Quell-IP:  10.125.1.121)
   Letzte erfolgr. Synchronisierungszeit: 22.04.2022 09:37:17
   Quelle: X000SV08.stl.leipzig.de
   Abrufintervall: 14 (16384s)

w32tm /query /configuration

       */

      /// <summary>
      /// holt die akt. Zeit von einem NTP-Server
      /// </summary>
      /// <param name="ntpServer"></param>
      /// <returns></returns>
      /// <exception cref="ArgumentNullException"></exception>
      public static async Task<DateTime?> GetNetworkTime(string ntpServer = "pool.ntp.org") {
         if (ntpServer == null)
            throw new ArgumentNullException(nameof(ntpServer));

         try {
            const long ticksPerSecond = 10000000L;

            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B; // 00 011 011: LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);   // NTP service - port number 123

            var pingDuration = Stopwatch.GetTimestamp(); // temp access (JIT-Compiler need some time at first call)
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
               await socket.ConnectAsync(ipEndPoint);
               socket.ReceiveTimeout = 5000;
               socket.Send(ntpData);
               pingDuration = Stopwatch.GetTimestamp(); // after Send-Method to reduce WinSocket API-Call time
               socket.Receive(ntpData);
               pingDuration = Stopwatch.GetTimestamp() - pingDuration;
            }
            long pingTicks = pingDuration * ticksPerSecond / Stopwatch.Frequency;

            /*
            System.Console.WriteLine("Reference Timestamp {0}", fromNTPBytes(ntpData, 16, pingTicks));
            System.Console.WriteLine("Origin    Timestamp {0}", fromNTPBytes(ntpData, 24, pingTicks));
            System.Console.WriteLine("Receive   Timestamp {0}", fromNTPBytes(ntpData, 32, pingTicks));
            System.Console.WriteLine("Transmit  Timestamp {0}", fromNTPBytes(ntpData, 40, pingTicks));
            */

            DateTime networkDateTime = fromNTPBytes(ntpData, 40, pingTicks);
            return networkDateTime.ToLocalTime(); // without ToLocalTime() = faster
         } catch (Exception ex) {
            System.Console.WriteLine(ex.ToString());
            return null;
         }
      }

      static DateTime fromNTPBytes(byte[] ntpData, int startidx = 40, long pingTicks = 0) {
         const int daysTo1900 = 1900 * 365 + 95; // 95 = offset for leap-years etc.
         const long ticksPerSecond = 10000000L;
         const long ticksPerDay = 24 * 60 * 60 * ticksPerSecond;
         const long ticksTo1900 = daysTo1900 * ticksPerDay;

         long intPart = (long)ntpData[startidx] << 24 | (long)ntpData[startidx + 1] << 16 | (long)ntpData[startidx + 2] << 8 | ntpData[startidx + 3];
         long fractPart = (long)ntpData[startidx + 4] << 24 | (long)ntpData[startidx + 5] << 16 | (long)ntpData[startidx + 6] << 8 | ntpData[startidx + 7];
         long netTicks = intPart * ticksPerSecond + (fractPart * ticksPerSecond >> 32);

         return new DateTime(ticksTo1900 + netTicks + pingTicks / 2);
      }
   }
}
