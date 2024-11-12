using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Война
{
    class Program
    {
        static void Main()
        {
            Headquarters headquarters = new Headquarters();
            Squad squad1 = headquarters.CreateSquad();
            Squad squad2 = headquarters.CreateSquad();

            Battlefield battlefield = new Battlefield();
            battlefield.Fight(squad1, squad2);
        }
    }

    class Soldier
    {
        private float _minDamageMultiplier = 0.25f;

        public Soldier(int health, int armor, int damage)
        {
            Health = health;
            Armor = armor;
            Damage = damage;
        }

        public bool IsAlive => Health > 0;
        protected int Health { get; private set; }
        protected int Armor { get; }
        protected int Damage { get; }

        public virtual void Attack(List<Soldier> soldiers)
        {
            int index = ChoseTarget(soldiers);

            soldiers[index].TakeDamage(Damage);
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage < 0)
                return;

            int resultDamage = Math.Max(damage - Armor, (int)(damage * _minDamageMultiplier));
            Health -= resultDamage;
        }


        protected virtual int ChoseTarget(List<Soldier> soldiers) =>
            RandomUtility.GenerateRandomValue(soldiers.Count);

        public virtual Soldier Clone() =>
            new Soldier(Health, Armor, Damage);
    }

    class Sniper : Soldier
    {
        private int _damageMultiplier;

        public Sniper(int health, int armor, int damage, int damageMultiplier) : base(health, armor, damage)
        {
            _damageMultiplier = damageMultiplier;
        }

        public override void Attack(List<Soldier> soldiers)
        {
            int index = ChoseTarget(soldiers);

            soldiers[index].TakeDamage(Damage * _damageMultiplier);
        }

        public override Soldier Clone() =>
            new Sniper(Health, Armor, Damage, _damageMultiplier);
    }

    class Grenadier : Soldier
    {
        public Grenadier(int health, int armor, int damage, int attacksQuantity) : base(health, armor, damage)
        {
            AttacksQuantity = attacksQuantity;
        }

        protected int AttacksQuantity { get; }

        public override void Attack(List<Soldier> soldiers)
        {
            List<Soldier> tempSoldiers = soldiers.ToList();

            int attackQuantity = Math.Min(AttacksQuantity, tempSoldiers.Count);

            for (int i = 0; i < attackQuantity; i++)
            {
                int index = ChoseTarget(tempSoldiers);
                Soldier soldier = tempSoldiers[index];

                soldier.TakeDamage(Damage);
                tempSoldiers.Remove(soldier);
            }
        }

        public override Soldier Clone() =>
            new Grenadier(Health, Armor, Damage, AttacksQuantity);
    }

    class Gunner : Grenadier
    {
        public Gunner(int health, int armor, int damage, int attacksQuantity) : base(health, armor, damage, attacksQuantity) { }

        public override void Attack(List<Soldier> soldiers)
        {
            int attackQuantity = Math.Min(AttacksQuantity, soldiers.Count);

            for (int i = 0; i < attackQuantity; i++)
                base.Attack(soldiers);
        }

        public override Soldier Clone() =>
            new Gunner(Health, Armor, Damage, AttacksQuantity);
    }

    class Squad
    {
        private List<Soldier> _soldiers;

        public Squad(List<Soldier> soldiers)
        {
            _soldiers = soldiers;
        }

        public bool IsAlive => _soldiers.Count > 0;
        public List<Soldier> Soldiers => _soldiers.ToList();

        public void Attack(List<Soldier> soldiers)
        {
            for (int i = 0; i < _soldiers.Count; i++)
                _soldiers[i].Attack(soldiers);
        }

        public void RemoveDead() =>
            _soldiers.RemoveAll(soldier => soldier.IsAlive == false);
    }

    class Headquarters
    {
        private List<Soldier> _soldiers;
        private int _soldiersQuantity = 10;

        public Headquarters()
        {
            _soldiers = FillSoldiers();
        }

        public Squad CreateSquad()
        {
            List<Soldier> soldiers = new List<Soldier>();

            foreach (Soldier soldier in _soldiers)
            {
                for (int i = 0; i < _soldiersQuantity; i++)
                {
                    soldiers.Add(soldier.Clone());
                }
            }

            return new Squad(soldiers);
        }

        private List<Soldier> FillSoldiers() =>
            new List<Soldier>()
            {
                new Soldier(100, 10, 10),
                new Sniper(100, 10, 10, 2),
                new Grenadier(100, 10, 10, 5),
                new Gunner(100, 10, 10, 5)
            };
    }

    class Battlefield
    {
        public void Fight(Squad squad1, Squad squad2)
        {
            while (squad1.IsAlive && squad2.IsAlive)
            {
                squad1.Attack(squad2.Soldiers);
                squad2.Attack(squad1.Soldiers);

                squad1.RemoveDead();
                squad2.RemoveDead();
            }

            WriteFightResult(squad1, squad2);
        }

        private void WriteFightResult(Squad squad1, Squad squad2)
        {
            if (squad1.IsAlive)
            {
                Console.WriteLine("Победил первый взвод.");

                return;
            }

            if (squad2.IsAlive)
            {
                Console.WriteLine("Победил второй взвод.");

                return;
            }

            Console.WriteLine("Оба взвода пали.");
        }
    }

    static class RandomUtility
    {
        private static Random s_random = new Random();

        public static int GenerateRandomValue(int maxValue) =>
            s_random.Next(maxValue);
    }
}
