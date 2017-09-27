using AlphaConfigurator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AlphaConfigurator.ManeuverUtil
{
    public class Movement
    {
        public int Yaw { get; set; }
        public int Pitch { get; set; }
        public int Roll { get; set; }

        public int TimeMs { get; set; }

        public string GetInSerialFormat()
        {
            return Roll.ZeroesUp(4) + "," + Pitch.ZeroesUp(4) + "," + Yaw.ZeroesUp(4) + "," + TimeMs.ZeroesUp(5) + "," + "z";
        }

        public Maneuver Host { get; set; }

        public ICommand RemoveCommand { get; set; }
        public ICommand MoveUpCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }

        public Movement(int yaw, int pitch, int roll, int time, Maneuver host)
        {
            Host = host;
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            TimeMs = time;

            RemoveCommand = new DelegateCommand(delegate (object arg)
            {
                Host.RemoveCommand.Execute(this);
            });
            MoveUpCommand = new DelegateCommand(delegate (object arg)
            {
                Host.MoveUpCommand.Execute(this);
            });
            MoveDownCommand = new DelegateCommand(delegate (object arg)
            {
                Host.MoveDownCommand.Execute(this);
            });
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Movement))
                return false;
            var other = obj as Movement;
            return other.Yaw == Yaw && other.Roll == Roll && other.Pitch == Pitch && other.TimeMs == TimeMs;
        }

        public override int GetHashCode()
        {
            var hashCode = -1766562337;
            hashCode = hashCode * -1521134295 + Yaw.GetHashCode();
            hashCode = hashCode * -1521134295 + Pitch.GetHashCode();
            hashCode = hashCode * -1521134295 + Roll.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeMs.GetHashCode();
            return hashCode;
        }
    }
}
