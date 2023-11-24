using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Есть 2 взвода. 1 взвод страны один, 2 взвод страны два.
//Каждый взвод внутри имеет солдат.
//Нужно написать программу, которая будет моделировать бой этих взводов.
//Каждый боец - это уникальная единица, он может иметь уникальные способности или же уникальные характеристики, такие как повышенная сила.
//Побеждает та страна, во взводе которой остались выжившие бойцы.
//Не важно, какой будет бой, рукопашный, стрелковый.

namespace Война
{
    class Program
    {
        static void Main()
        {
            Headquarters headquarters = new Headquarters();
            Battlefield battlefield = new Battlefield(headquarters.CreateSquad(), headquarters.CreateSquad());

            battlefield.Fight();
        }
    }

    class Soldier : IComparable<Soldier>
    {
        private int _health;
        private int _damage;

        public Soldier(int health, int damage)
        {
            _health = health;
            _damage = damage;
        }

        public bool IsAlive => _health > 0;

        public int Attack() => _damage;//урон сделать свойством?

        public void TakeDamage(int damage) => _health -= damage;

        public int CompareTo(Soldier soldier) => IsAlive.CompareTo(soldier.IsAlive);
    }

    class SoldierFabric
    {
        private int _minHealth = 80;
        private int _maxHealth = 100;
        private int _minDamage = 10;
        private int _maxDamage = 20;

        public Soldier Create() => new Soldier(RandomUtility.Next(_minHealth, _maxHealth), RandomUtility.Next(_minDamage, _maxDamage));
    }

    class Squad
    {
        private Soldier[] _soldiers;

        public Squad(Soldier[] soldiers) => _soldiers = soldiers;

        public bool IsAlive => _soldiers.Any(soldier => soldier.IsAlive);

        public void Attack(Squad squad)
        {
            Array.Sort(_soldiers);

            for (int i = 0; i < _soldiers.Length; i++)
                squad.TakeDamage(i, _soldiers[i].Attack());
        }

        public void TakeDamage(int index, int damage) => _soldiers[index].TakeDamage(damage);
    }

    class Headquarters
    {
        private int _squadSize = 30;
        private SoldierFabric _soldierFabric = new SoldierFabric();

        public Squad CreateSquad()
        {
            Soldier[] soldiers = new Soldier[_squadSize];

            for (int i = 0; i < soldiers.Length; i++)
                soldiers[i] = _soldierFabric.Create();

            return new Squad(soldiers);
        }
    }

    class Battlefield
    {
        private Squad _squad1;
        private Squad _squad2;

        public Battlefield(Squad squad1, Squad squad2)
        {
            _squad1 = squad1;
            _squad2 = squad2;
        }

        public void Fight()
        {
            while (_squad1.IsAlive && _squad2.IsAlive)
            {
                _squad1.Attack(_squad2);
                _squad2.Attack(_squad1);
            }

            Console.WriteLine(_squad1.IsAlive);
            Console.WriteLine(_squad2.IsAlive);
        }
    }

    static class RandomUtility
    {
        static private Random s_random = new Random();

        static public int Next(int value) => s_random.Next(value);

        static public int Next(int minValue, int maxValue) => s_random.Next(minValue, maxValue);
    }
}
