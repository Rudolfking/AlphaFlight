using AlphaConfigurator.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AlphaConfigurator.ManeuverUtil
{
    public class Maneuver
    {
        [JsonProperty("Movements")]
        public ObservableCollection<Movement> Movements { get; set; } = new ObservableCollection<Movement>();

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICommand AddCommand { get; set; }

        [JsonIgnore]
        public ICommand RemoveCommand { get; set; }
        [JsonIgnore]
        public ICommand MoveUpCommand { get; set; }
        [JsonIgnore]
        public ICommand MoveDownCommand { get; set; }

        private static int uids = 1;
        [JsonProperty("Uid")]
        public int Uid { get; private set; }

        public Maneuver()
        {
            initCommands();
        }

        public Maneuver(Maneuver copy, bool isDuplicate)
        {
            Uid = uids++;

            if (isDuplicate)
                Name = copy.Name + "- Copy";
            else
                Name = copy.Name;
            Movement[] target = new Movement[copy.Movements.Count];
            copy.Movements.CopyTo(target, 0);
            this.Movements = new ObservableCollection<Movement>();
            foreach (var item in target)
            {
                item.Host = this;
                Movements.Add(item);
            }

            initCommands();
        }

        public Maneuver(string name)
        {
            Uid = uids++;

            Name = name;

            initCommands();
        }

        private void initCommands()
        {
            AddCommand = new DelegateCommand(delegate (object arg)
            {
                var man = arg as Movement;
                Movements.Add(man);
            });
            RemoveCommand = new DelegateCommand(delegate (object arg)
            {
                var man = arg as Movement;
                Movements.Remove(man);
            });
            MoveUpCommand = new DelegateCommand(delegate (object arg)
            {
                var man = arg as Movement;
                for (int i = 1; i < Movements.Count; i++)
                {
                    if (man == Movements[i])
                    {
                        var ki = Movements[i];
                        Movements[i] = Movements[i - 1];
                        Movements[i - 1] = ki;
                        break;
                    }
                }
            });
            MoveDownCommand = new DelegateCommand(delegate (object arg)
            {
                var man = arg as Movement;
                for (int i = 0; i < Movements.Count - 1; i++)
                {
                    if (man == Movements[i])
                    {
                        var ki = Movements[i];
                        Movements[i] = Movements[i + 1];
                        Movements[i + 1] = ki;
                        break;
                    }
                }
            });
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Maneuver))
                return false;
            var other = obj as Maneuver;
            if (other.Name != Name)
                return false;
            if (other.Uid != Uid)
                return false;

            if (other.Movements.Count != Movements.Count)
                return false;

            if (!Movements.All(x => other.Movements.Contains(x)))
                return false;

            return true;
        }

        internal static void UpdateUid(int newUid)
        {
            uids = newUid;
        }

        internal string GetPrettyMovementsText()
        {
            var ret = "";
            foreach (var item in this.Movements)
            {
                ret += item.GetInSerialFormat().Replace(',', ' ');
                ret += Environment.NewLine;
            }
            return ret;
        }

        public override int GetHashCode()
        {
            return -1737426059 + Uid.GetHashCode();
        }
    }
}
