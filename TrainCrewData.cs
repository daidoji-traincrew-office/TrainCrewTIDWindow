using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrainCrewTIDWindow {
    [Serializable]
    public class CommandToTrainCrew {
        [JsonInclude]
        public string command = "";
        [JsonInclude]
        public string[] args = [];
    }

    [Serializable]
    public class DataFromTrainCrew {
        public TimeData nowTime;
        public TrainState myTrainData = new TrainState();
        public List<TrackCircuitData>? trackCircuitList;
        public List<RunTrainData>? otherTrainDataList;
        public List<SignalData>? signalDataList;

        public GameScreen gameScreen = GameScreen.Other;
        public CrewType crewType = CrewType.Driver;
        public DriveMode driveMode = DriveMode.Normal;
    }

    [Serializable]
    public struct TimeData {
        public int hour;
        public int minute;
        public float second;

        public TimeData(int h, int m, float s) {
            hour = h;
            minute = m;
            second = s;
        }

        public static TimeData FromTimeSpan(TimeSpan ts) {
            return new TimeData(ts.Hours, ts.Minutes, ts.Seconds + ts.Milliseconds / 1000f);
        }
        public TimeSpan ToTimeSpan() {
            return new TimeSpan(0, hour, minute, (int)second, (int)((second % 1f) * 1000));
        }

        public override string ToString() {
            return hour + ":" + minute + ":" + second.ToString("0.0");
        }
    }


    [Serializable]
    public class TrackCircuitData {
        public bool On = false;
        public string Last = "";//軌道回路を踏んだ列車の名前
        public string Name = "";

        public override string ToString() {
            return $"{Name}/{Last}/{On}";
        }
    }

    public enum DriveMode {
        Normal,
        Free,
        RTA,
    }
    public enum CrewType {
        Driver,
        Conductor,
        Passenger,
    }

    public enum GameScreen {
        MainGame,
        MainGame_Pause,
        MainGame_Loading,
        Menu,
        Result,
        Title,
        Other,
        NotRunningGame,
    }

    [Serializable]
    public class RunTrainData {
        public string Name = "";
        public string Class = "";
        public string BoundFor = "";
        public bool onTrack = false; //出発
        public bool autoDriveEnable = false; //出発
        public float Speed;
        public float speedTo = 110;
        public bool AllClose;
        public float TotalLength = 0;
        public bool isJieiR = false;
        public string debugMsg = "";
    }


    [Serializable]
    public class SignalData {
        public string Name = "";
        public Phase phase = Phase.None;
    }

    public enum Phase {
        None,
        R,
        YY,
        Y,
        YG,
        G
    }

    [Serializable]
    public class TrainState {
        public float Speed;
        public bool AllClose;
        public float MR_Press;
        public List<CarState> CarStates = new List<CarState>();

        public Dictionary<PanelLamp, bool> Lamps = new Dictionary<PanelLamp, bool>();
        public string ATS_Class = "普通";
        public string ATS_Speed = "110";
        public string ATS_State = "無表示";

        public string diaName = "";
        public string Class = "";
        public string BoundFor = "";

        public float nextUIDistance = 0;
        public float nextStaDistance = 0;
        public string nextStaName = "";
        public string nextStopType = "停車";
        public float speedLimit = 110;
        public float nextSpeedLimit = -1;
        public float nextSpeedLimitDistance = -1;
        public float gradient = 0;
        public float TotalLength = 0;

        public int Pnotch = 0;
        public int Bnotch = 0;
        public int Reverser = 1;

        public List<StationInfo> stationList = new List<StationInfo>();
        public int nowStaIndex = 0;

        public TrainState() {
            foreach (PanelLamp lmp in Enum.GetValues(typeof(PanelLamp))) {
                Lamps[lmp] = (lmp == PanelLamp.ATS_Ready);
            }
        }
    }
    [Serializable]
    public class CarState {
        public bool DoorClose;
        public float BC_Press;
        public float Ampare;
        public string CarModel = "";
        public bool HasPantograph = false;
        public bool HasDriverCab = false;
        public bool HasConductorCab = false;
        public bool HasMotor = false;
    }
    [Serializable]
    public class StationInfo {
        public string Name = "";
        public string StopPosName = "";
        public TimeData ArvTime;
        public TimeData DepTime;
        public string doorDir = "";
        public string stopType = "";
        public float TotalLength = 0;
    }

    [Serializable]
    public enum PanelLamp {
        /// <summary>
        /// ●戸閉
        /// </summary>
        DoorClose,
        /// <summary>
        /// ATS正常
        /// </summary>
        ATS_Ready,
        /// <summary>
        /// ATS動作
        /// </summary>
        ATS_BrakeApply,
        /// <summary>
        /// ATS開放
        /// </summary>
        ATS_Open,
        /// <summary>
        /// 回生
        /// </summary>
        RegenerativeBrake,
        /// <summary>
        /// EB
        /// </summary>
        EB_Timer,
        /// <summary>
        /// 非常ブレーキ
        /// </summary>
        EmagencyBrake,
        /// <summary>
        /// 過負荷
        /// </summary>
        Overload,
    }
}
