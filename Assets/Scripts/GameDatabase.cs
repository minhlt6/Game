using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public enum HeroId
    {
        Mercy = 0,
        Inirius = 1,
        Eni = 2
    }

    public enum WeaponType
    {
        MainPhysical = 0,
        MainMagic = 1,
        SubPhysical = 2,
        SubMagic = 3
    }

    public enum MonsterCategory
    {
        Regular = 1,
        MiniBoss = 2,
        FinalBoss = 3
    }

    [Serializable]
    public class HeroDefinition
    {
        public HeroId Id;
        public string CodeName;
        public string AssetFolder;
        public string PortraitSprite;
        public string DisplayName;
        public string RoleTitle;
        public string LoreDescription;
        public string StartingWeaponId;

        public float BaseHP;
        public float HPPerLevel;

        public float BaseAttack; // Strength or Magic/ATK
        public float AttackPerLevel;
        public string AttackStatLabel;

        public float BaseEndurance;
        public float EndurancePerLevel;

        public float BaseSpeed;
        public float SpeedPerLevel;

        public string ProgressionSummary;
    }

    [Serializable]
    public class WeaponDefinition
    {
        public string Id;
        public string DisplayName;
        public WeaponType Category;
        public string IconSprite;
        public string EffectAnimFolder;
        public string Description;
        public string LevelUpNote;

        public float BaseDamage;
        public float BaseCooldown;
        public float DamageBonusPerLevel; // e.g., +0.15f per additional level
        public float RangeOrRadiusBonusPerLevel;
        public int ExtraProjectilesPerLevel;
        public float CooldownReductionPerLevel;
        public float SlowPercentBase;
        public float SlowPercentPerLevel;
        public bool IsContinuousAura;
    }

    [Serializable]
    public class PassiveSkillDefinition
    {
        public string Id;
        public string DisplayName;
        public string IconSprite;
        public string DescriptionLv1;
        public string DescriptionLv2;
        public int MaxLevel = 2;
    }

    [Serializable]
    public class DropItemDefinition
    {
        public string Id;
        public string DisplayName;
        public string IconSprite;
        public string AnimFolder;
        public string Description;
        public string DropSource;
    }

    [Serializable]
    public class MonsterDefinition
    {
        public string Id;
        public string DisplayName;
        public string PrimaryAnimFolder;
        public string SecondaryAnimFolder;
        public string PortraitSprite;
        public MonsterCategory Category;
        public int[] StagesAppearing;
        public float MaxHP;
        public float Strength;
        public float Endurance;
        public float Speed;
        public int ExpReward;
        public int GoldReward;
        public string AttackPatternDescription;
        public bool HasRangedAttack;
        public bool HasChargeAttack;
        public bool HasAoEAttack;
        public bool AppliesBurn;
        public bool AppliesBleed;
        public bool SummonsMinions;
        public int SpawnKillThreshold;
    }

    [Serializable]
    public class StageDefinition
    {
        public int StageNumber;
        public string ChapterCode;
        public string Title;
        public string Subtitle;
        public string LoreDescription;
        public string PreviewSprite;
        public string GroundSprite;
        public string PathSprite;
        public string BgmTrack;
        public int MiniBossKillsRequired;
        public int FinalBossKillsRequired;
        public string MiniBossId;
        public string FinalBossId;
        public string[] RegularMonsterIds;
        public Color AmbientTint;
    }

    public static class GameDatabase
    {
        public static readonly List<HeroDefinition> Heroes = new List<HeroDefinition>
        {
            new HeroDefinition
            {
                Id = HeroId.Mercy,
                CodeName = "Mercy",
                AssetFolder = "normal_nuri",
                PortraitSprite = "Sprites/hero_mercy",
                DisplayName = "Mercy (normal_nuri)",
                RoleTitle = "Chiến binh cận chiến",
                LoreDescription = "Nữ chiến binh dũng cảm xứ Astral, luôn xung phong tiến lên phía trước với thanh Kiếm Ánh Sáng để bảo vệ đồng đội.",
                StartingWeaponId = "weapon_00",
                BaseHP = 150f,
                HPPerLevel = 12f,
                BaseAttack = 18f,
                AttackPerLevel = 3f,
                AttackStatLabel = "Strength",
                BaseEndurance = 12f,
                EndurancePerLevel = 2f,
                BaseSpeed = 4.2f,
                SpeedPerLevel = 0.05f,
                ProgressionSummary = "Lv1: Kiếm Ánh Sáng Lv1 | Lv3: Rìu Chiến Xoay / Cường Lực 1 | Lv5: Kiếm Ánh Sáng Lv2 / Giáp Cứng 1 | Lv10: Hào Quang Thức Tỉnh / Đại Đao Phán Quyết | Lv15: Hồi Phục 1 / Kiếm Lv4"
            },
            new HeroDefinition
            {
                Id = HeroId.Inirius,
                CodeName = "Inirius",
                AssetFolder = "normal_durun",
                PortraitSprite = "Sprites/hero_inirius",
                DisplayName = "Inirius (normal_durun)",
                RoleTitle = "Pháp sư nguyên tố",
                LoreDescription = "Pháp sư thông thái - bộ não của nhóm mạo hiểm gia, sở hữu các phép thuật nguyên tố lửa và sấm sét có sức công phá hủy diệt.",
                StartingWeaponId = "weapon_04",
                BaseHP = 100f,
                HPPerLevel = 8f,
                BaseAttack = 25f,
                AttackPerLevel = 4f,
                AttackStatLabel = "Magic/ATK",
                BaseEndurance = 6f,
                EndurancePerLevel = 1f,
                BaseSpeed = 4.5f,
                SpeedPerLevel = 0.05f,
                ProgressionSummary = "Lv1: Trượng Nguyên Tố Lv1 | Lv3: Thiên Lôi / Cường Lực 1 | Lv5: Trượng Nguyên Tố Lv2 (+tia lửa) | Lv10: Cuồng Phong / Thần Tốc 1 | Lv15: Hào Quang Thức Tỉnh / Trượng Lv4"
            },
            new HeroDefinition
            {
                Id = HeroId.Eni,
                CodeName = "Eni",
                AssetFolder = "normal_mokoji",
                PortraitSprite = "Sprites/hero_eni",
                DisplayName = "Eni (normal_mokoji)",
                RoleTitle = "Phù thủy kiểm soát",
                LoreDescription = "Nữ phù thủy chuyên về kiểm soát không gian và hỗ trợ, có vẻ ngoài nhỏ bé nhưng làm những điều phi thường với Sách Phép Cổ Đại.",
                StartingWeaponId = "weapon_06_1",
                BaseHP = 110f,
                HPPerLevel = 9f,
                BaseAttack = 16f,
                AttackPerLevel = 2.5f,
                AttackStatLabel = "Magic/ATK",
                BaseEndurance = 8f,
                EndurancePerLevel = 1.5f,
                BaseSpeed = 4.8f,
                SpeedPerLevel = 0.08f,
                ProgressionSummary = "Lv1: Sách Phép Cổ Đại Lv1 (Freezing) | Lv3: Phi Tiêu Bóng Đêm / Từ Trường 1 | Lv5: Sách Phép Lv2 / Hồi Phục 1 | Lv10: Cuồng Phong / Thiên Lôi | Lv15: Thần Tốc 2 / Sách Phép Lv4"
            }
        };

        public static readonly Dictionary<string, WeaponDefinition> Weapons = new Dictionary<string, WeaponDefinition>
        {
            {
                "weapon_00",
                new WeaponDefinition
                {
                    Id = "weapon_00",
                    DisplayName = "Kiếm Ánh Sáng",
                    Category = WeaponType.MainPhysical,
                    IconSprite = "Sprites/weapon_00",
                    EffectAnimFolder = "",
                    Description = "(Đa mục tiêu - Vật lý) Tự động chém quét cận chiến theo hướng di chuyển của nhân vật.",
                    LevelUpNote = "Mỗi cấp: +15% sát thương, +10% tầm quét",
                    BaseDamage = 65f,
                    BaseCooldown = 1.2f,
                    DamageBonusPerLevel = 0.15f,
                    RangeOrRadiusBonusPerLevel = 0.10f
                }
            },
            {
                "weapon_04",
                new WeaponDefinition
                {
                    Id = "weapon_04",
                    DisplayName = "Trượng Nguyên Tố",
                    Category = WeaponType.MainMagic,
                    IconSprite = "Sprites/weapon_04",
                    EffectAnimFolder = "burn",
                    Description = "(Đa mục tiêu - Phép) Tự động phóng cầu lửa nguyên tố (fire / flame) theo hướng di chuyển.",
                    LevelUpNote = "Mỗi cấp: +20% sát thương, thêm +1 tia lửa",
                    BaseDamage = 75f,
                    BaseCooldown = 1.4f,
                    DamageBonusPerLevel = 0.20f,
                    ExtraProjectilesPerLevel = 1
                }
            },
            {
                "weapon_06_1",
                new WeaponDefinition
                {
                    Id = "weapon_06_1",
                    DisplayName = "Sách Phép Cổ Đại",
                    Category = WeaponType.MainMagic,
                    IconSprite = "Sprites/weapon_06_1",
                    EffectAnimFolder = "freezing",
                    Description = "(Đa mục tiêu - Phép) Bắn tia ma thuật theo hướng di chuyển và làm chậm (freezing) kẻ địch.",
                    LevelUpNote = "Mỗi cấp: +15% sát thương, +10% làm chậm",
                    BaseDamage = 55f,
                    BaseCooldown = 1.0f,
                    DamageBonusPerLevel = 0.15f,
                    SlowPercentBase = 0.25f,
                    SlowPercentPerLevel = 0.10f
                }
            },
            {
                "weapon_02_1",
                new WeaponDefinition
                {
                    Id = "weapon_02_1",
                    DisplayName = "Rìu Chiến Xoay",
                    Category = WeaponType.SubPhysical,
                    IconSprite = "Sprites/weapon_02_1",
                    EffectAnimFolder = "",
                    Description = "(Vũ khí phụ - Vật lý) Phóng rìu xoay theo quỹ đạo vòng cung xuyên qua đám đông quái vật.",
                    LevelUpNote = "Mỗi cấp: +18% sát thương, thêm +1 rìu",
                    BaseDamage = 70f,
                    BaseCooldown = 2.0f,
                    DamageBonusPerLevel = 0.18f,
                    ExtraProjectilesPerLevel = 1
                }
            },
            {
                "weapon_05_4",
                new WeaponDefinition
                {
                    Id = "weapon_05_4",
                    DisplayName = "Phi Tiêu Bóng Đêm",
                    Category = WeaponType.SubPhysical,
                    IconSprite = "Sprites/weapon_05_4",
                    EffectAnimFolder = "",
                    Description = "(Vũ khí phụ - Vật lý) Phóng liên tiếp phi tiêu tốc độ cao theo hướng di chuyển của nhân vật.",
                    LevelUpNote = "Mỗi cấp: +15% sát thương, thêm +1 phi tiêu",
                    BaseDamage = 45f,
                    BaseCooldown = 0.7f,
                    DamageBonusPerLevel = 0.15f,
                    ExtraProjectilesPerLevel = 1
                }
            },
            {
                "weapon_08",
                new WeaponDefinition
                {
                    Id = "weapon_08",
                    DisplayName = "Đại Đao Phán Quyết",
                    Category = WeaponType.SubPhysical,
                    IconSprite = "Sprites/weapon_08",
                    EffectAnimFolder = "",
                    Description = "(Vũ khí phụ - Vật lý) Chém mạnh tạo sóng xung kích sát thương lớn và đẩy lùi quái vật phía trước.",
                    LevelUpNote = "Mỗi cấp: +25% sát thương, giảm 0.2s hồi chiêu",
                    BaseDamage = 95f,
                    BaseCooldown = 2.5f,
                    DamageBonusPerLevel = 0.25f,
                    CooldownReductionPerLevel = 0.2f
                }
            },
            {
                "hurricane",
                new WeaponDefinition
                {
                    Id = "hurricane",
                    DisplayName = "Cuồng Phong",
                    Category = WeaponType.SubMagic,
                    IconSprite = "Sprites/weapon_hurricane",
                    EffectAnimFolder = "hurricane",
                    Description = "(Vũ khí phụ - Phép) Tạo lốc xoáy di chuyển theo hướng nhìn, cuốn và gây sát thương liên tục.",
                    LevelUpNote = "Mỗi cấp: +15% sát thương, +15% bán kính",
                    BaseDamage = 60f,
                    BaseCooldown = 2.8f,
                    DamageBonusPerLevel = 0.15f,
                    RangeOrRadiusBonusPerLevel = 0.15f
                }
            },
            {
                "thunderbolt",
                new WeaponDefinition
                {
                    Id = "thunderbolt",
                    DisplayName = "Thiên Lôi",
                    Category = WeaponType.SubMagic,
                    IconSprite = "Sprites/weapon_thunderbolt",
                    EffectAnimFolder = "thunderbolt",
                    Description = "(Vũ khí phụ - Phép) Giáng sấm sét ngẫu nhiên xuống các mục tiêu quái vật xung quanh nhân vật.",
                    LevelUpNote = "Mỗi cấp: +20% sát thương, thêm +1 tia sét",
                    BaseDamage = 90f,
                    BaseCooldown = 2.2f,
                    DamageBonusPerLevel = 0.20f,
                    ExtraProjectilesPerLevel = 1
                }
            },
            {
                "awaken_aura",
                new WeaponDefinition
                {
                    Id = "awaken_aura",
                    DisplayName = "Hào Quang Thức Tỉnh",
                    Category = WeaponType.SubMagic,
                    IconSprite = "Sprites/weapon_awaken_aura",
                    EffectAnimFolder = "awaken_aura",
                    Description = "(Vũ khí phụ - Phép) Tạo vòng năng lượng quanh thân, đốt cháy mọi quái vật tiếp cận gần.",
                    LevelUpNote = "Duy trì liên tục | Mỗi cấp: +15% sát thương, +12% bán kính",
                    BaseDamage = 35f,
                    BaseCooldown = 0.5f,
                    DamageBonusPerLevel = 0.15f,
                    RangeOrRadiusBonusPerLevel = 0.12f,
                    IsContinuousAura = true
                }
            }
        };

        public static readonly Dictionary<string, PassiveSkillDefinition> PassiveSkills = new Dictionary<string, PassiveSkillDefinition>
        {
            {
                "cuong_luc",
                new PassiveSkillDefinition
                {
                    Id = "cuong_luc",
                    DisplayName = "Cường Lực",
                    IconSprite = "Sprites/skill_cuong_luc",
                    DescriptionLv1 = "Cường Lực 1: Tăng +15% sát thương đầu ra cho toàn bộ vũ khí chính và vũ khí phụ.",
                    DescriptionLv2 = "Cường Lực 2: Tăng +30% sát thương đầu ra cho toàn bộ vũ khí chính và vũ khí phụ."
                }
            },
            {
                "than_toc",
                new PassiveSkillDefinition
                {
                    Id = "than_toc",
                    DisplayName = "Thần Tốc",
                    IconSprite = "Sprites/skill_than_toc",
                    DescriptionLv1 = "Thần Tốc 1: Tăng +10% tốc độ di chuyển của nhân vật và tốc độ ra đòn của vũ khí.",
                    DescriptionLv2 = "Thần Tốc 2: Tăng +20% tốc độ di chuyển của nhân vật và tốc độ ra đòn của vũ khí."
                }
            },
            {
                "giap_cung",
                new PassiveSkillDefinition
                {
                    Id = "giap_cung",
                    DisplayName = "Giáp Cứng",
                    IconSprite = "Sprites/skill_giap_cung",
                    DescriptionLv1 = "Giáp Cứng 1: Tăng +5 chỉ số Endurance (Giáp), giảm sát thương nhận vào.",
                    DescriptionLv2 = "Giáp Cứng 2: Tăng +10 chỉ số Endurance (Giáp), giảm sát thương nhận vào."
                }
            },
            {
                "hoi_phuc",
                new PassiveSkillDefinition
                {
                    Id = "hoi_phuc",
                    DisplayName = "Hồi Phục",
                    IconSprite = "Sprites/skill_hoi_phuc",
                    DescriptionLv1 = "Hồi Phục 1: Tự động hồi phục 2% máu tối đa mỗi 3 giây trong màn chơi.",
                    DescriptionLv2 = "Hồi Phục 2: Tự động hồi phục 4% máu tối đa mỗi 3 giây trong màn chơi."
                }
            },
            {
                "tu_truong",
                new PassiveSkillDefinition
                {
                    Id = "tu_truong",
                    DisplayName = "Từ Trường",
                    IconSprite = "Sprites/skill_tu_truong",
                    DescriptionLv1 = "Từ Trường 1: Tăng +30% bán kính thu thập hạt kinh nghiệm (energy_capsules) và vật phẩm.",
                    DescriptionLv2 = "Từ Trường 2: Tăng +60% bán kính thu thập hạt kinh nghiệm (energy_capsules) và vật phẩm."
                }
            }
        };

        public static readonly Dictionary<string, MonsterDefinition> Monsters = new Dictionary<string, MonsterDefinition>
        {
            // Regular Monsters (Loại 1)
            {
                "slime",
                new MonsterDefinition
                {
                    Id = "slime",
                    DisplayName = "Slime (slime / emerald_slime)",
                    PrimaryAnimFolder = "slime",
                    SecondaryAnimFolder = "emerald_slime",
                    PortraitSprite = "Sprites/mob_slime",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 1 },
                    MaxHP = 100f,
                    Strength = 6f,
                    Endurance = 3f,
                    Speed = 2.5f,
                    ExpReward = 2,
                    GoldReward = 5,
                    AttackPatternDescription = "Đánh thường (Áp sát va chạm)"
                }
            },
            {
                "goblin",
                new MonsterDefinition
                {
                    Id = "goblin",
                    DisplayName = "Goblin (axe_goblin / bomber_goblin)",
                    PrimaryAnimFolder = "axe_goblin",
                    SecondaryAnimFolder = "bomber_goblin",
                    PortraitSprite = "Sprites/mob_goblin",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 1 },
                    MaxHP = 180f,
                    Strength = 10f,
                    Endurance = 5f,
                    Speed = 3.5f,
                    ExpReward = 4,
                    GoldReward = 10,
                    AttackPatternDescription = "Đánh thường, Bổ rìu / Ném bom nổ tầm ngắn",
                    HasRangedAttack = true
                }
            },
            {
                "wild_boar",
                new MonsterDefinition
                {
                    Id = "wild_boar",
                    DisplayName = "Wild Boar (wild_boar / angry_wild_boar)",
                    PrimaryAnimFolder = "wild_boar",
                    SecondaryAnimFolder = "angry_wild_boar",
                    PortraitSprite = "Sprites/mob_wild_boar",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 1 },
                    MaxHP = 240f,
                    Strength = 14f,
                    Endurance = 7f,
                    Speed = 4.2f,
                    ExpReward = 6,
                    GoldReward = 20,
                    AttackPatternDescription = "Đánh thường, Húc thẳng tốc độ cao",
                    HasChargeAttack = true
                }
            },
            {
                "skeleton",
                new MonsterDefinition
                {
                    Id = "skeleton",
                    DisplayName = "Skeleton (skeleton_warrior / archer)",
                    PrimaryAnimFolder = "skeleton_warrior",
                    SecondaryAnimFolder = "skeleton_archer",
                    PortraitSprite = "Sprites/mob_skeleton",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 2 },
                    MaxHP = 280f,
                    Strength = 18f,
                    Endurance = 12f,
                    Speed = 3.6f,
                    ExpReward = 10,
                    GoldReward = 40,
                    AttackPatternDescription = "Đánh thường, Chém kiếm / Bắn tên tầm xa",
                    HasRangedAttack = true
                }
            },
            {
                "vampire_bat",
                new MonsterDefinition
                {
                    Id = "vampire_bat",
                    DisplayName = "Vampire Bat & Ghost",
                    PrimaryAnimFolder = "vampire_bat",
                    SecondaryAnimFolder = "gloomy_ghost",
                    PortraitSprite = "Sprites/mob_vampire_bat",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 2 },
                    MaxHP = 250f,
                    Strength = 20f,
                    Endurance = 10f,
                    Speed = 4.5f,
                    ExpReward = 14,
                    GoldReward = 60,
                    AttackPatternDescription = "Đánh thường, Lao cắn hút máu",
                    HasChargeAttack = true
                }
            },
            {
                "gargoyle",
                new MonsterDefinition
                {
                    Id = "gargoyle",
                    DisplayName = "Gargoyle & Evil Skull",
                    PrimaryAnimFolder = "angry_gargoyle",
                    SecondaryAnimFolder = "evil_skull",
                    PortraitSprite = "Sprites/mob_gargoyle",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 2, 3 },
                    MaxHP = 360f,
                    Strength = 26f,
                    Endurance = 18f,
                    Speed = 3.8f,
                    ExpReward = 24,
                    GoldReward = 100,
                    AttackPatternDescription = "Đánh thường, Xung kích / Lửa địa ngục (burn)",
                    HasChargeAttack = true,
                    AppliesBurn = true
                }
            },
            {
                "lava_slime",
                new MonsterDefinition
                {
                    Id = "lava_slime",
                    DisplayName = "Lava Slime & Demon Skull",
                    PrimaryAnimFolder = "lava_slime",
                    SecondaryAnimFolder = "demon_skull",
                    PortraitSprite = "Sprites/mob_lava_slime",
                    Category = MonsterCategory.Regular,
                    StagesAppearing = new[] { 3 },
                    MaxHP = 450f,
                    Strength = 32f,
                    Endurance = 22f,
                    Speed = 4.2f,
                    ExpReward = 32,
                    GoldReward = 160,
                    AttackPatternDescription = "Đánh thường, Phun dung nham & Tia lửa (flame)",
                    HasRangedAttack = true,
                    AppliesBurn = true
                }
            },

            // Mini Bosses (Loại 2) & Final Bosses (Loại 3)
            {
                "boss_goblin",
                new MonsterDefinition
                {
                    Id = "boss_goblin",
                    DisplayName = "Goblin Boss / Tree Monster",
                    PrimaryAnimFolder = "axe_goblin_vari_boss",
                    SecondaryAnimFolder = "tree_monster",
                    PortraitSprite = "Sprites/boss_goblin",
                    Category = MonsterCategory.MiniBoss,
                    StagesAppearing = new[] { 1 },
                    MaxHP = 1500f,
                    Strength = 20f,
                    Endurance = 10f,
                    Speed = 3.5f,
                    ExpReward = 50,
                    GoldReward = 150,
                    SpawnKillThreshold = 100,
                    AttackPatternDescription = "Đánh thường, Quăng rìu lớn / Quất rễ cây diện rộng",
                    HasRangedAttack = true,
                    HasAoEAttack = true
                }
            },
            {
                "boss_golem",
                new MonsterDefinition
                {
                    Id = "boss_golem",
                    DisplayName = "Golem (mr_rock)",
                    PrimaryAnimFolder = "mr_rock",
                    SecondaryAnimFolder = "minotauros",
                    PortraitSprite = "Sprites/boss_golem",
                    Category = MonsterCategory.FinalBoss,
                    StagesAppearing = new[] { 1 },
                    MaxHP = 3500f,
                    Strength = 30f,
                    Endurance = 18f,
                    Speed = 3.0f,
                    ExpReward = 100,
                    GoldReward = 200,
                    SpawnKillThreshold = 250,
                    AttackPatternDescription = "Đánh thường, Bạo kích đập đất gây chấn động diện rộng",
                    HasAoEAttack = true,
                    HasChargeAttack = true
                }
            },
            {
                "boss_bone_gargoyle",
                new MonsterDefinition
                {
                    Id = "boss_bone_gargoyle",
                    DisplayName = "Bone Gargoyle / Crimson",
                    PrimaryAnimFolder = "bone_gargoyle",
                    SecondaryAnimFolder = "crimson_murderer",
                    PortraitSprite = "Sprites/boss_bone_gargoyle",
                    Category = MonsterCategory.MiniBoss,
                    StagesAppearing = new[] { 2 },
                    MaxHP = 2400f,
                    Strength = 28f,
                    Endurance = 14f,
                    Speed = 4.2f,
                    ExpReward = 80,
                    GoldReward = 250,
                    SpawnKillThreshold = 150,
                    AttackPatternDescription = "Đánh thường, Lao vồ tốc độ cao / Chém gây chảy máu",
                    HasChargeAttack = true,
                    AppliesBleed = true
                }
            },
            {
                "boss_grim_reaper",
                new MonsterDefinition
                {
                    Id = "boss_grim_reaper",
                    DisplayName = "Grim Reaper (dark_murderer)",
                    PrimaryAnimFolder = "dark_murderer",
                    SecondaryAnimFolder = "darkvane",
                    PortraitSprite = "Sprites/boss_grim_reaper",
                    Category = MonsterCategory.FinalBoss,
                    StagesAppearing = new[] { 2 },
                    MaxHP = 4500f,
                    Strength = 40f,
                    Endurance = 20f,
                    Speed = 4.5f,
                    ExpReward = 200,
                    GoldReward = 400,
                    SpawnKillThreshold = 350,
                    AttackPatternDescription = "Đánh thường, Lướt nhanh chém lưỡi hái tử thần gây chảy máu (bleeding)",
                    HasChargeAttack = true,
                    HasAoEAttack = true,
                    AppliesBleed = true
                }
            },
            {
                "boss_guardian_fire",
                new MonsterDefinition
                {
                    Id = "boss_guardian_fire",
                    DisplayName = "Guardian of Fire / Drake",
                    PrimaryAnimFolder = "guardian_of_fire",
                    SecondaryAnimFolder = "drake",
                    PortraitSprite = "Sprites/boss_guardian_fire",
                    Category = MonsterCategory.MiniBoss,
                    StagesAppearing = new[] { 3 },
                    MaxHP = 3600f,
                    Strength = 45f,
                    Endurance = 22f,
                    Speed = 4.0f,
                    ExpReward = 150,
                    GoldReward = 350,
                    SpawnKillThreshold = 200,
                    AttackPatternDescription = "Đánh thường, Phun cầu lửa liên hoàn và vòng lửa đốt cháy",
                    HasRangedAttack = true,
                    HasAoEAttack = true,
                    AppliesBurn = true
                }
            },
            {
                "boss_herald_of_death",
                new MonsterDefinition
                {
                    Id = "boss_herald_of_death",
                    DisplayName = "Herald of Death (kaiser_demon)",
                    PrimaryAnimFolder = "kaiser_demon",
                    SecondaryAnimFolder = "n_kaiser_demon",
                    PortraitSprite = "Sprites/boss_herald_of_death",
                    Category = MonsterCategory.FinalBoss,
                    StagesAppearing = new[] { 3 },
                    MaxHP = 6500f,
                    Strength = 55f,
                    Endurance = 28f,
                    Speed = 4.6f,
                    ExpReward = 500,
                    GoldReward = 1000,
                    SpawnKillThreshold = 500,
                    AttackPatternDescription = "Đánh thường, Bão lửa (flame), Thiên lôi (thunderbolt) & Triệu hồi quái",
                    HasRangedAttack = true,
                    HasAoEAttack = true,
                    AppliesBurn = true,
                    SummonsMinions = true
                }
            }
        };

        public static readonly List<StageDefinition> Stages = new List<StageDefinition>
        {
            new StageDefinition
            {
                StageNumber = 1,
                ChapterCode = "chapter_1",
                Title = "Màn 1: Khu Rừng Astral",
                Subtitle = "Vùng rừng xanh cổ đại bị Golem và bè lũ Goblin chiếm đóng",
                LoreDescription = "Khu rừng hùng vĩ tươi đẹp của vùng đất Astral nhưng nay bị chiếm đóng bởi đàn Slime, Goblin và Heo rừng hung dữ dưới trướng Golem.",
                PreviewSprite = "Sprites/preview_stage_1",
                GroundSprite = "Maps/ground_stage_1",
                PathSprite = "Maps/ground_stage_1_path",
                BgmTrack = "Audio/bgm_stage1",
                MiniBossKillsRequired = 100,
                FinalBossKillsRequired = 250,
                MiniBossId = "boss_goblin",
                FinalBossId = "boss_golem",
                RegularMonsterIds = new[] { "slime", "goblin", "wild_boar" },
                AmbientTint = new Color(0.95f, 1.0f, 0.92f, 1f)
            },
            new StageDefinition
            {
                StageNumber = 2,
                ChapterCode = "chapter_5",
                Title = "Màn 2: Lâu Đài Bóng Tối",
                Subtitle = "Pháo đài cổ âm u canh gác bởi tử thần Grim Reaper",
                LoreDescription = "Lâu đài tối tăm đáng sợ ẩn sâu giữa rừng già, nơi trú ngụ của chiến binh xương, dơi hút máu, tượng đá Gargoyle và thần chết Grim Reaper.",
                PreviewSprite = "Sprites/preview_stage_2",
                GroundSprite = "Maps/ground_stage_2",
                PathSprite = "Maps/ground_stage_2_path",
                BgmTrack = "Audio/bgm_stage2",
                MiniBossKillsRequired = 150,
                FinalBossKillsRequired = 350,
                MiniBossId = "boss_bone_gargoyle",
                FinalBossId = "boss_grim_reaper",
                RegularMonsterIds = new[] { "skeleton", "vampire_bat", "gargoyle" },
                AmbientTint = new Color(0.82f, 0.88f, 0.98f, 1f)
            },
            new StageDefinition
            {
                StageNumber = 3,
                ChapterCode = "chapter_7",
                Title = "Màn 3: Cổng Địa Ngục",
                Subtitle = "Vực lửa dung nham - Quyết chiến Trùm cuối Herald of Death",
                LoreDescription = "Cổng Địa Ngục rực lửa dung nham, nơi ác quỷ cấp cao Herald of Death đang mở rộng cánh cổng ma giới chuẩn bị xâm lăng thế giới loài người.",
                PreviewSprite = "Sprites/preview_stage_3",
                GroundSprite = "Maps/ground_stage_3",
                PathSprite = "Maps/ground_stage_3_path",
                BgmTrack = "Audio/bgm_stage3",
                MiniBossKillsRequired = 200,
                FinalBossKillsRequired = 500,
                MiniBossId = "boss_guardian_fire",
                FinalBossId = "boss_herald_of_death",
                RegularMonsterIds = new[] { "gargoyle", "lava_slime" },
                AmbientTint = new Color(1.0f, 0.86f, 0.80f, 1f)
            }
        };

        /// <summary>
        /// Công thức tính sát thương Vũ khí chính / Vũ khí phụ (Phần 3.1):
        /// Sát thương = (Chỉ số cơ bản của Vũ khí + Chỉ số Strength/Magic của nhân vật) x (1 + 0,2 x Cấp độ Vũ khí) x Hệ số kỹ năng Cường Lực
        /// </summary>
        public static float CalculateWeaponDamage(float weaponBaseStat, float heroStrengthOrMagic, int weaponLevel, int cuongLucSkillLevel)
        {
            int effectiveLevel = Mathf.Max(1, weaponLevel);
            float cuongLucMultiplier = 1.0f;
            if (cuongLucSkillLevel == 1) cuongLucMultiplier = 1.15f;
            else if (cuongLucSkillLevel >= 2) cuongLucMultiplier = 1.30f;

            return (weaponBaseStat + heroStrengthOrMagic) * (1.0f + 0.2f * effectiveLevel) * cuongLucMultiplier;
        }

        /// <summary>
        /// Công thức tính sát thương nhân vật nhận vào khi bị quái vật hoặc Boss tấn công (Phần 3.1):
        /// Sát thương nhận vào = Tối thiểu 1, hoặc bằng: Sát thương của Quái vật - (Chỉ số Endurance của nhân vật x 0,8)
        /// </summary>
        public static float CalculateDamageTaken(float monsterStrength, float heroEndurance)
        {
            float raw = monsterStrength - (heroEndurance * 0.8f);
            return Mathf.Max(1.0f, raw);
        }

        /// <summary>
        /// Các mốc EXP tích lũy cần có để nhân vật lên cấp (Đã điều chỉnh chậm lại ~30 EXP/cấp đầu trận, ~30-40s chiến đấu mỗi cấp):
        /// - Level 1 - 5: 30 EXP mỗi level (Các mốc EXP: 30 - 60 - 90 - 120 - 150)
        /// - Level 5 - 10: 60 EXP mỗi level (Các mốc EXP: 210 - 270 - 330 - 390 - 450)
        /// - Level 10 - 20: 100 EXP mỗi level (Các mốc EXP: 550 - 650 - ... - 1450)
        /// - Level 20 - 30: 180 EXP mỗi level (Các mốc EXP: 1630 - 1810 - ... - 3250)
        /// </summary>
        public static int GetCumulativeExpThreshold(int milestoneIndex)
        {
            if (milestoneIndex <= 0) return 0;
            if (milestoneIndex <= 5)
            {
                return milestoneIndex * 30; // 30, 60, 90, 120, 150
            }
            if (milestoneIndex <= 10)
            {
                return 150 + (milestoneIndex - 5) * 60; // 210, 270, 330, 390, 450
            }
            if (milestoneIndex <= 20)
            {
                return 450 + (milestoneIndex - 10) * 100; // 550 .. 1450
            }
            return 1450 + (milestoneIndex - 20) * 180; // 1630 .. 3250+
        }

        public static int GetIncrementalExpForLevel(int currentLevel)
        {
            int idx = Mathf.Max(1, currentLevel);
            return GetCumulativeExpThreshold(idx) - GetCumulativeExpThreshold(idx - 1);
        }

        public static HeroDefinition GetHero(HeroId id)
        {
            foreach (var h in Heroes)
            {
                if (h.Id == id) return h;
            }
            return Heroes[0];
        }

        public static StageDefinition GetStage(int stageNumber)
        {
            foreach (var s in Stages)
            {
                if (s.StageNumber == stageNumber) return s;
            }
            return Stages[0];
        }
    }
}
