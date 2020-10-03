using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HitPointTracker_API.CharacterSheet;

namespace HitPointTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HitPointTrackerController : ControllerBase
    {
        private readonly CharacterSheetContext _context;

        public HitPointTrackerController(CharacterSheetContext context)
        {
            _context = context;
        }

        // GET: api/HitPointTracker
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacterData>>> GetCharacterData()
        {
            //return await _context.CharacterData.ToListAsync();
            var characterData = _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).AsNoTracking();
            return await characterData.ToListAsync(); 
        }

        // GET: api/HitPointTracker/FindCharacter/Defaulty
        [HttpGet("FindCharacter/{id}")]
        public async Task<ActionResult<CharacterData>> GetCharacterData(string id)
        {
            var characterData =  _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            return await characterData;
        }

        // GET: api/HitPointTracker/CharacterList
        [HttpGet("CharacterList")]
        public async Task<ActionResult<IEnumerable<String>>> GetCharacterNameData(string id)
        {
            var characterData =  _context.CharacterData.Select(x => x.Name).AsNoTracking();
            return await characterData.ToListAsync();
        }

        // GET: api/HitPointTracker/CharacterHealth/Defaulty
        [HttpGet("CharacterHealth/{id}")]
        public async Task<ActionResult<int>> GetCharacterHealthData(string id)
        {
            var healthValue =  _context.CharacterData.Where(x => x.Name == id).Select(x => x.Health).Select(y => y.HitPointsCurrent).FirstOrDefaultAsync();

            if (healthValue == null)
            {
                return NotFound();
            }

            return await healthValue;
        }

        // PUT: api/HitPointTracker/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacterData(string id, CharacterData characterData)
        {
            //Console.WriteLine("PUT Character: " + characterData.Health.HitPointsCurrent.ToString());
            if (id != characterData.Name)
            {
                return BadRequest();
            }
            characterData.AdjustRollMethod(characterData.RollMethod != null ? characterData.RollMethod : "default");
            characterData.ApplyBonuses();
            _context.Update(characterData);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacterDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/HitPointTracker/RollMethod/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("RollMethod/{id}")]
        public async Task<IActionResult> PutRollMethodCharacter(string id, [FromBody]string rollMethod)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.AdjustRollMethod(rollMethod);
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/Heal/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("Heal/{id}")]
        public async Task<IActionResult> PutHealCharacter(string id, [FromBody]int heal = 0)
        {
            //Console.WriteLine("Healing: " + heal.ToString());
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.Health.ApplyHealing(heal);
            //Console.WriteLine("Health After: " + characterData.Health.HitPointsCurrent.ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/Damage/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("Damage/{id}")]
        public async Task<IActionResult> PutDamageCharacter(string id, [FromBody]Damage dmg)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);
            if (characterData == null)
            {
                return NotFound();
            }
            var characterDefense = _context.CharacterData.Where(x => x.Name == id).FirstOrDefault().Defenses.ToList();
            if(characterDefense.Count() > 0)
            {
                foreach(DefensesData _data in characterDefense)
                {
                    dmg.Value = _data.ModDamage(dmg);
                }
            }
            characterData.Health.ApplyDamage(dmg.Value);
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/MaxHitPointsMod/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("MaxHitPointsMod/{id}")]
        public async Task<IActionResult> PutHitPointsMaxCharacter(string id, [FromBody]int mod = 0)
        {
            //Console.WriteLine("Mod: " + mod.ToString());
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }
            characterData.Health.ModifyHitPointMax(mod);
            //Console.WriteLine("Max Health After: " + characterData.Health.GetHitPointMax().ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/ResetMaxHitPoints/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("ResetMaxHitPoints/{id}")]
        public async Task<IActionResult> PutResetHitPointsMaxCharacter(string id)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.Health.ResetHitPointMax();
            //Console.WriteLine("Max Health After: " + characterData.Health.GetHitPointMax().ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/ResetHitPoints/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("ResetHitPoints/{id}")]
        public async Task<IActionResult> PutResetHitPointsCharacter(string id)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.Health.ResetHitPoints();
            //Console.WriteLine("Health After: " + characterData.Health.HitPointsCurrent.ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/UpdateTempHitPoints/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("UpdateTempHitPoints/{id}")]
        public async Task<IActionResult> PutTempHitPointsCharacter(string id, [FromBody]int value = 0)
        {
            //Console.WriteLine("Value: " + value.ToString());
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }
            characterData.Health.UpdateTempHitPoints(value);
            //Console.WriteLine("Temp Health After: " + characterData.Health.TemporaryHitPoints.ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/ResetTempHitPoints/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("ResetTempHitPoints/{id}")]
        public async Task<IActionResult> PutResetTempHitPointsCharacter(string id)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.Health.ResetTempHitPoints();
            //Console.WriteLine("Health After: " + characterData.Health.HitPointsCurrent.ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/RerollHitPoints/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("RerollHitPoints/{id}")]
        public async Task<IActionResult> PutRerollHitPointsCharacter(string id)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }

            characterData.RollAllHitPoints();
            //Console.WriteLine("Max Health After: " + characterData.Health.GetHitPointMax().ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // PUT: api/HitPointTracker/LevelUp/Defaulty
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("LevelUp/{id}")]
        public async Task<IActionResult> PutLevelUpCharacter(string id, [FromBody]string className)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);

            if (characterData == null)
            {
                return NotFound();
            }
            var characterClass = characterData.Classes.FirstOrDefault(x => x.Name == className.ToLower());
            if(characterClass == null)
            {
                return NotFound();
            }
            characterData.LevelUp(characterClass.Name);
            //Console.WriteLine("Health After: " + characterData.Health.HitPointsCurrent.ToString());
            return await PutCharacterData(characterData.Name, characterData);
        }

        // POST: api/HitPointTracker
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<CharacterData>> PostCharacterData(CharacterData characterData)
        {
            if (CharacterDataExists(characterData.Name))
            {
                return Conflict();
            }
            characterData.AdjustRollMethod(characterData.RollMethod != null ? characterData.RollMethod : "default");
            characterData.ApplyBonuses();
            _context.CharacterData.Add(characterData);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CharacterDataExists(characterData.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCharacterData", new { id = characterData.Name }, characterData);
        }

        // DELETE: api/HitPointTracker/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CharacterData>> DeleteCharacterData(string id)
        {
            var characterData = await _context.CharacterData.Include(x => x.Stats).Include(x => x.Health).Include(x => x.Bonuses).Include(x => x.Classes).Include(x => x.Defenses).Include(x => x.Items).FirstOrDefaultAsync(y => y.Name == id);
            
            if (characterData == null)
            {
                return NotFound();
            }

            _context.CharacterData.Remove(characterData);
            await _context.SaveChangesAsync();

            return characterData;
        }

        private bool CharacterDataExists(string id)
        {
            return _context.CharacterData.Any(e => e.Name == id);
        }
    }
}
