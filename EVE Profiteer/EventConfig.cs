
namespace eZet.EveProfiteer {
    public static class EventConfig {

        //public class TraceType {
        //    public const int Start = 1;
        //    public const int Complete = 2;
        //    public const int Debug = 3;
        //    public const int Warning = 4;
        //    public const int Error = 5;
        //    public const int Stop = 8;
        //    public const int FataError = 9;
        //}

        //public class Area {
        //    public const int Syntax = 0;
        //    public const int System = 1;
        //    public const int Connection = 2;
        //    public const int Authentication = 3;
        //    public const int Error = 5;
        //    public const int Stop = 8;
        //    public const int Unknown = 9;
        //}

        public const int StartInitializerBootstrapper = 1200;
        public const int StatusChanged = 1201;



        public static void Init() {
            EventManager.AddRange(
                Event(StartInitializerBootstrapper, "StartInitializeBootstrapper"),
                Event(StatusChanged, "StatusChanged: {0}")

                

                );
        }

        public static TraceEvent Event(int id, string format) {
            return new TraceEvent(id, "", format);
        }


    }
}
