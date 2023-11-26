using System;
using System.Collections.Generic;
using System.Linq;

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

    class Soldier
    {
        private int _damage;
        private int _currentHealth;
        private int _maxHealth;

        public Soldier(int health, int damage, int attackRange)
        {
            _maxHealth = health;
            _currentHealth = health;
            _damage = damage;
            AttackRange = attackRange;
        }

        public int AttackRange { get; private set; }
        public bool IsAlive => _currentHealth > 0;
        public bool IsHealthFull => _currentHealth == _maxHealth;

        public virtual void Attack(Soldier enemy) =>
            enemy.TakeDamage(_damage);

        public void GetTreatment(int healValue)
        {
            _currentHealth += healValue;

            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
        }

        public void TakeDamage(int damage) =>
            _currentHealth -= damage;

        public int Aim(List<int> targetIndexes) =>
            targetIndexes[RandomUtility.Next(targetIndexes.Count)];
    }

    class Medic : Soldier
    {
        public readonly int healRange = 1;
        private int _valueOfHeal;

        public Medic(int health, int damage, int attackRange, int valueOfHeal) : base(health, damage, attackRange) =>
            _valueOfHeal = valueOfHeal;

        public void Heal(Soldier soldier) =>
            soldier.GetTreatment(_valueOfHeal);
    }

    class Grenadier : Soldier
    {
        public readonly int grenadeAttackRange = 1;
        private int _grenadeDamage = 30;
        private int _grenadeCooldown = 3;
        private int _grenadeCooldownCounter = 0;

        public Grenadier(int health, int damage, int attackRange) : base(health, damage, attackRange) { }

        public bool IsGrenadeAttackReady => _grenadeCooldownCounter == _grenadeCooldown;

        public void GrenadeAttack(List<Soldier> enemies)
        {
            enemies.ForEach(enemy => enemy.TakeDamage(_grenadeDamage));

            _grenadeCooldownCounter = 0;
        }

        public override void Attack(Soldier soldier)
        {
            base.Attack(soldier);

            _grenadeCooldownCounter++;
        }
    }

    class SoldierFabric
    {
        private int[] _healthStats = { 80, 100 };
        private int[] _damageStats = { 15, 20 };
        private int[] _attackRangeStats = { 1, 3 };
        private int[] _healStats = { 10, 15 };

        public Soldier CreateSoldier() => new Soldier(
            RandomUtility.Next(_healthStats),
            RandomUtility.Next(_damageStats),
            RandomUtility.Next(_attackRangeStats));

        public Medic CreateMedic() => new Medic(
            RandomUtility.Next(_healthStats),
            RandomUtility.Next(_damageStats),
            RandomUtility.Next(_attackRangeStats),
            RandomUtility.Next(_healStats));

        public Grenadier CreateGrenadier() => new Grenadier(
            RandomUtility.Next(_healthStats),
            RandomUtility.Next(_damageStats),
            RandomUtility.Next(_attackRangeStats));
    }

    class Squad
    {
        private List<Soldier> _soldiers;

        public Squad(List<Soldier> soldiers) =>
            _soldiers = soldiers;

        public int Size => _soldiers.Count;
        public bool IsAlive => _soldiers.Any(soldier => soldier.IsAlive);

        public void Attack(Squad enemiesSquad)
        {
            for (int i = 0; i < _soldiers.Count; i++)
            {
                List<int> targetsIndexes;

                if (_soldiers[i] is Medic medic)
                {
                    targetsIndexes = GiveMedicTargets(i, medic.healRange);

                    if (targetsIndexes.Any())
                    {
                        int targetIndex = medic.Aim(targetsIndexes);

                        medic.Heal(_soldiers[targetIndex]);
                    }
                }

                if (_soldiers[i] is Grenadier grenadier && grenadier.IsGrenadeAttackReady)
                {
                    List<Soldier> enemies = GiveGrenadierTargets(i, grenadier.AttackRange, enemiesSquad);

                    grenadier.GrenadeAttack(enemies);
                }

                targetsIndexes = GiveSoldierTargets(_soldiers[i], enemiesSquad, i);

                if (targetsIndexes.Any())
                {
                    int enemyIndex = _soldiers[i].Aim(targetsIndexes);

                    Soldier enemy = enemiesSquad.GiveSoldierByIndex(enemyIndex);

                    _soldiers[i].Attack(enemy);
                }
            }
        }

        public void RemoveDead() =>
            _soldiers.FindAll(soldier => soldier.IsAlive == false).ForEach(soldier => _soldiers.Remove(soldier));

        private bool IsSoldierByIndexAlive(int index) =>
            _soldiers[index].IsAlive;

        private Soldier GiveSoldierByIndex(int index) =>
            _soldiers[index];

        private List<int> GiveSoldierTargets(Soldier soldier, Squad enemiesSquad, int soldierIndex)
        {
            List<int> enemiesIndexes = new List<int>();

            int[] indexRange = CreateIndexRange(soldierIndex, soldier.AttackRange, enemiesSquad.Size);

            for (int i = indexRange[0]; i < indexRange[1]; i++)
                if (enemiesSquad.IsSoldierByIndexAlive(i))
                    enemiesIndexes.Add(i);

            return enemiesIndexes;
        }

        private List<int> GiveMedicTargets(int medicIndex, int healRange)
        {
            List<int> targetsIndexes = new List<int>();

            int[] indexRange = CreateIndexRange(medicIndex, healRange, _soldiers.Count);

            for (int i = indexRange[0]; i < indexRange[1]; i++)
                if (_soldiers[i].IsHealthFull == false)
                    targetsIndexes.Add(i);

            return targetsIndexes;
        }

        private List<Soldier> GiveGrenadierTargets(int grenadierIndex, int grenadeAttackRange, Squad enemiesSquad)
        {
            List<Soldier> enemies = new List<Soldier>();

            int[] indexRange = CreateIndexRange(grenadierIndex, grenadeAttackRange, enemiesSquad.Size);

            for (int i = indexRange[0]; i < indexRange[1]; i++)
                enemies.Add(enemiesSquad.GiveSoldierByIndex(i));

            return enemies;
        }

        private int[] CreateIndexRange(int currentIndex, int range, int maxIndex)
        {
            int[] indexRange = new int[2];

            int startIndex = currentIndex - range;
            int lastIndex = currentIndex + range;

            if (startIndex < 0)
                startIndex = 0;

            if (lastIndex > maxIndex)
                lastIndex = maxIndex;

            indexRange[0] = startIndex;
            indexRange[1] = lastIndex;

            return indexRange;
        }
    }

    class Headquarters
    {
        private int[] _squadSizeRange = { 25, 30 };
        private SoldierFabric _soldierFabric = new SoldierFabric();

        private int _medicsQuantity = 5;
        private int _grenadierQuantity = 5;

        public Squad CreateSquad()
        {
            int squadSize = RandomUtility.Next(_squadSizeRange);

            List<Soldier> soldiers = new List<Soldier>();

            for (int i = 0; i < _medicsQuantity; i++)
                soldiers.Add(_soldierFabric.CreateMedic());

            for (int i = 0; i < _grenadierQuantity; i++)
                soldiers.Add(_soldierFabric.CreateGrenadier());

            for (int i = soldiers.Count; i < squadSize; i++)
                soldiers.Add(_soldierFabric.CreateSoldier());

            Shuffle(soldiers);

            return new Squad(soldiers);
        }

        private void Shuffle(List<Soldier> soldiers)
        {
            Soldier tempSoldier;

            for (int i = 0; i < soldiers.Count; i++)
            {
                int index = RandomUtility.Next(soldiers.Count);

                tempSoldier = soldiers[index];
                soldiers[index] = soldiers[i];
                soldiers[i] = tempSoldier;
            }
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

                _squad1.RemoveDead();
                _squad2.RemoveDead();
            }

            WriteFightResult();
        }

        private void WriteFightResult()
        {
            if (_squad1.IsAlive == false && _squad2.IsAlive == false)
                Console.WriteLine("Оба взвода пали.");
            else if (_squad1.IsAlive)
                Console.WriteLine("Победил первый взвод.");
            else
                Console.WriteLine("Победил второй взвод.");
        }
    }

    static class RandomUtility
    {
        static private Random s_random = new Random();

        static public int Next(int value) =>
            s_random.Next(value);

        static public int Next(int[] valueRange) =>
            s_random.Next(valueRange[0], valueRange[1] + 1);
    }
}
