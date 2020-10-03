using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class CharacterData
{
    [Key]
    public string Name { get; set; }
    public int Level { get; set; }
    public string RollMethod { get; set; }
    public HealthData Health { get; set; }
    public ICollection<ClassData> Classes { get; set; }
    public StatsData Stats { get; set; }
    public ICollection<ItemsData> Items { get; set; }
    public ICollection<DefensesData> Defenses { get; set; }
    public ICollection<BonusesData> Bonuses { get; set; }

    public void ApplyBonuses()
    {
        ClearBonuses();
        if (Bonuses != null)
            foreach (var bonus in Bonuses)
            {
                switch (bonus.AffectedObject.ToLower())
                {
                    case "health":
                        this.Health.ApplyBonus(bonus);
                        break;
                    case "stats":
                        this.Stats.ApplyBonus(bonus);
                        break;
                    default:
                        break;
                }
            }
        if (Items != null)
            foreach (var bonus in Items)
            {
                switch (bonus.AffectedObject.ToLower())
                {
                    case "health":
                        this.Health.ApplyBonus(bonus);
                        break;
                    case "stats":
                        this.Stats.ApplyBonus(bonus);
                        break;
                    default:
                        break;
                }
            }
    }

    public void ClearBonuses()
    {
        this.Health.ApplyBonus(new BonusesData() {AffectedObject = "Health", AffectedValue = "clear"});
        this.Stats.ApplyBonus(new BonusesData() {AffectedObject = "Health", AffectedValue = "clear"});
    }

    public void AdjustRollMethod(string _rollMethod)
    {
        switch (_rollMethod.ToLower())
        {
            case "roll":
                this.RollMethod = "roll";
                break;
            case "round":
            default:
                this.RollMethod = "round";
                break;
        }
    }

    public int RollHitPoints(ClassData _charClass)
    {
        if (_charClass == null)
            return 0;
        var hitPoints = this.Health.HitDieMod + this.Stats.GetConstitutionMod();
        //Console.WriteLine("Class: " + _charClass.Name);
        //Console.WriteLine("HD: d"+_charClass.HitDie.ToString());
        //Console.WriteLine("HitPoints before Roll: " + hitPoints.ToString());
        switch (this.RollMethod)
        {
            case "roll":
                hitPoints += RollDie(_charClass.HitDie);
                break;
            case "round":
            default:
                hitPoints += Convert.ToInt32(_charClass.HitDie / 2.0) + 1;
                break;
        }
        //Console.WriteLine("HitPoints After Roll: " + hitPoints.ToString());
        return hitPoints;
    }

    public void RollAllHitPoints()
    {
        var HitPointMax = 0;
        foreach (var charClass in this.Classes)
        {
            for (int level = 0; level < charClass.Level; level++)
            {
                HitPointMax += this.RollHitPoints(charClass);
            }
        }
        //Console.WriteLine("Final Max HitPoints: "+ HitPointMax.ToString());
        this.Health.HitPointsMax = HitPointMax;
    }
    public void LevelUp(string _charClassName)
    {
        foreach (var charClass in this.Classes)
        {
            if (charClass.Name == _charClassName)
            {
                charClass.Level += 1;
                this.Level += 1;
                this.Health.HitPointsMax += this.RollHitPoints(charClass);
                return;
            }
        }
    }
    int RollDie(int Die)
    {
        Random rand = new Random();
        return rand.Next(1, Die);
    }
}
public class HealthData
{
    [Key]
    public int HealthDataId { get; set; }
    public int HitPointsMax { get; set; }
    public int HitPointsCurrent { get; set; }
    public int HitPointsMaxMod { get; set; }
    public int TemporaryHitPoints { get; set; }
    public int HitDieMod { get; set; }

    public int GetHitPointMax()
    {
        return this.HitPointsMax + this.HitPointsMaxMod;
    }
    public bool IsUnconscious()
    {
        if (this.HitPointsCurrent <= 0)
            return true;
        return false;
    }

    public void ResetHitPointMax()
    {
        this.HitPointsMaxMod = 0;
        if(this.HitPointsCurrent > this.GetHitPointMax())
            this.HitPointsCurrent = this.GetHitPointMax();
    }
    public void ResetHitPoints()
    {
        this.HitPointsCurrent = this.GetHitPointMax();
    }
    public void ResetTempHitPoints()
    {
        this.TemporaryHitPoints = 0;
    }
    public void ModifyHitPointMax(int _mod)
    {
        this.HitPointsMaxMod += _mod;
        if (this.HitPointsMaxMod < -this.HitPointsMax)
            this.HitPointsMaxMod = -this.HitPointsMax;
        if(this.HitPointsCurrent > this.GetHitPointMax())
            this.HitPointsCurrent = this.GetHitPointMax();
    }
    public void AdjustHitPoints(int _value)
    {
        this.HitPointsCurrent += _value;
        if (this.HitPointsCurrent > this.GetHitPointMax())
            this.HitPointsCurrent = this.GetHitPointMax();
        if (this.HitPointsCurrent < 0)
            this.HitPointsCurrent = 0;
    }
    public void DamageTempHitPoints(int _dmg)
    {
        this.TemporaryHitPoints -= _dmg;
        if (this.TemporaryHitPoints < 0)
            this.TemporaryHitPoints = 0;
    }
    public void UpdateTempHitPoints(int _value)
    {
        if (this.TemporaryHitPoints < _value)
            this.TemporaryHitPoints = _value;
    }
    public void ApplyDamage(int _dmg)
    {
        if(_dmg<0)
            return;
        int _remainder = _dmg;
        if (this.TemporaryHitPoints > 0)
        {
            _remainder = _dmg - this.TemporaryHitPoints;
            DamageTempHitPoints(_dmg);
            if (_remainder < 0)
                _remainder = 0;
        }
        AdjustHitPoints(-_remainder);
    }
    public void ApplyHealing(int _health)
    {
        if(_health<0)
            return;
        AdjustHitPoints(_health);
    }
    public void ApplyBonus(BonusesData _bonus)
    {
        switch (_bonus.AffectedValue.ToLower())
        {
            case "hitdiemod":
                this.HitDieMod += _bonus.Value;
                break;
            case "clear":
                this.HitDieMod = 0;
                break;
            default:
                break;
        }
    }
    public void ApplyBonus(ItemsData _bonus)
    {
        switch (_bonus.AffectedValue.ToLower())
        {
            case "hitdiemod":
                this.HitDieMod += _bonus.Value;
                break;
            case "clear":
                this.HitDieMod = 0;
                break;
            default:
                break;
        }
    }
}

public class ClassData
{
    [Key]
    public int ClassDataId { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int HitDie { get; set; }
}

public class StatsData
{
    [Key]
    public int StatsDataId { get; set; }
    public int Strength { get; set; }
    public int MiscStrengthAdj { get; set; }
    public int Dexterity { get; set; }
    public int MiscDexterityAdj { get; set; }
    public int Constitution { get; set; }
    public int MiscConstitutionAdj { get; set; }
    public int Intelligence { get; set; }
    public int MiscIntelligenceAdj { get; set; }
    public int Wisdom { get; set; }
    public int MiscWisdomAdj { get; set; }
    public int Charisma { get; set; }
    public int MiscCharismaAdj { get; set; }

    public void ApplyBonus(BonusesData _bonus)
    {
        switch (_bonus.AffectedValue.ToLower())
        {
            case "strength":
                this.MiscStrengthAdj += _bonus.Value;
                break;
            case "dexterity":
                this.MiscDexterityAdj += _bonus.Value;
                break;
            case "constitution":
                this.MiscConstitutionAdj += _bonus.Value;
                break;
            case "intelligence":
                this.MiscIntelligenceAdj += _bonus.Value;
                break;
            case "wisdom":
                this.MiscWisdomAdj += _bonus.Value;
                break;
            case "charisma":
                this.MiscCharismaAdj += _bonus.Value;
                break;
            case "clear":
                this.MiscStrengthAdj = 0;
                this.MiscDexterityAdj = 0;
                this.MiscConstitutionAdj = 0;
                this.MiscIntelligenceAdj = 0;
                this.MiscWisdomAdj = 0;
                this.MiscCharismaAdj = 0;
                break;
            default:
                break;
        }
    }
    public void ApplyBonus(ItemsData _bonus)
    {
        switch (_bonus.AffectedValue.ToLower())
        {
            case "strength":
                this.MiscStrengthAdj += _bonus.Value;
                break;
            case "dexterity":
                this.MiscDexterityAdj += _bonus.Value;
                break;
            case "constitution":
                this.MiscConstitutionAdj += _bonus.Value;
                break;
            case "intelligence":
                this.MiscIntelligenceAdj += _bonus.Value;
                break;
            case "wisdom":
                this.MiscWisdomAdj += _bonus.Value;
                break;
            case "charisma":
                this.MiscCharismaAdj += _bonus.Value;
                break;
            default:
                break;
        }
    }
    public int GetAdjustedStrength()
    {
        return this.Strength + this.MiscStrengthAdj;
    }
    public int GetAdjustedDexterity()
    {
        return this.Dexterity + this.MiscDexterityAdj;
    }
    public int GetAdjustedConstitution()
    {
        return this.Constitution + this.MiscConstitutionAdj;
    }
    public int GetAdjustedIntelligence()
    {
        return this.Intelligence + this.MiscIntelligenceAdj;
    }
    public int GetAdjustedWisdom()
    {
        return this.Wisdom + this.MiscWisdomAdj;
    }
    public int GetAdjustedCharisma()
    {
        return this.Charisma + this.MiscCharismaAdj;
    }

    public int GetStrengthMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedStrength() - 10.0) / 2.0));
    }
    public int GetDexterityMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedDexterity() - 10.0) / 2.0));
    }
    public int GetConstitutionMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedConstitution() - 10.0) / 2.0));
    }
    public int GetIntelligenceMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedIntelligence() - 10.0) / 2.0));
    }
    public int GetWisdomMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedWisdom() - 10.0) / 2.0));
    }
    public int GetCharismaMod()
    {
        return Convert.ToInt32(Math.Floor((this.GetAdjustedCharisma() - 10.0) / 2.0));
    }
}
public class ItemsData
{
    [Key]
    public int ItemsDataId { get; set; }
    public string Name { get; set; }
    public string AffectedObject { get; set; }
    public string AffectedValue { get; set; }
    public int Value { get; set; }
}
public class BonusesData
{
    [Key]
    public int BonusesDataId { get; set; }
    public string Name { get; set; }
    public string AffectedObject { get; set; }
    public string AffectedValue { get; set; }
    public int Value { get; set; }
}

public class ModifiersData
{
    [Key]
    public int ModifiersDataId { get; set; }
    public string AffectedObject { get; set; }
    public string AffectedValue { get; set; }
    public int Value { get; set; }
}

public class DefensesData
{
    [Key]
    public int DefensesDataId { get; set; }
    public string Type { get; set; }
    public string Defense { get; set; }
    public int ModDamage(Damage _dmg)
    {
        if (this.Type.ToLower() == _dmg.Type.ToLower())
        {
            switch (this.Defense.ToLower())
            {
                case "immunity":
                    return 0;
                case "resistance":
                    return Convert.ToInt32(Math.Floor(_dmg.Value / 2.0));
                case "vulnerability":
                    return Convert.ToInt32(_dmg.Value * 2);
                default:
                    break;
            }
        }
        return _dmg.Value;
    }
}

public struct Damage
{
    public string Type { get; set; }
    public int Value { get; set; }
}