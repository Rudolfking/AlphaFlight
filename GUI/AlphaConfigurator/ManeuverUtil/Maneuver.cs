using AlphaConfigurator.Utils;
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
        public ObservableCollection<Movement> Movements { get; set; } = new ObservableCollection<Movement>();

        public string Name { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand RemoveCommand { get; set; }
        public ICommand MoveUpCommand { get; set; }
        public ICommand MoveDownCommand { get; set; }

        private static int uids = 1;
        public int Uid { get; private set; }

        public Maneuver(Maneuver copy)
        {
            Uid = uids++;

            Name = copy.Name + "- Copy";
            Movement[] target = new Movement[copy.Movements.Count];
            copy.Movements.CopyTo(target, 0);
            this.Movements = new ObservableCollection<Movement>();
            foreach (var item in target)
            {
                Movements.Add(item);
            }
        }

        public Maneuver(string name)
        {
            Uid = uids++;

            Name = name;

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

        public override int GetHashCode()
        {
            var hashCode = -1121205505;
            hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<Movement>>.Default.GetHashCode(Movements);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode + Uid.GetHashCode();
        }
    }
}
