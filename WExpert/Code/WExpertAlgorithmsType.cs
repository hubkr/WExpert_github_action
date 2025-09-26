using System.ComponentModel;

namespace WExpert.Code;

public enum WExpertAlgorithmsType
{
    NONE = -1,
    /* 보형물 정보(Implant Information) */
    [Description("pocket_position")]
    POCKET_POSITION = 0,         // Subglandular & Subpectoral
    [Description("shell_type")]
    SHELL_TYPE,                  // Smooth & Texture
    [Description("shape_type")]
    SHAPE_TYPE,                  // Round & Anatomical
    [Description("manufacturer")]
    MANUFACTURER,               // Allergan & Mentor & Silimed & Polytech & Sebbin
    [Description("constituent")]
    CONSTITUENT,                // 성분
    /* 부작용(Complication) */
    [Description("rupture")]
    RUPTURE,                    // 정상/파열
    [Description("folding")]
    FOLDING,                    // 보형물 접힘 유무
    [Description("fluid_collection")]
    FLUID_COLLECTION,            // (장액종) 유무(보형물 주변 장액종)
    [Description("thickened_capsule")]
    THICKENED_CAPSULE,           // (피막 뚜꺼워짐) 유무
    [Description("upside_down_rotation")]
    UPSIDE_DOWN_ROTATION,         // (보형물 뒤집어짐) 유무
    [Description("capsular_mass")]
    CAPSULAR_MASS,               // 피막 결절
    [Description("capsular_calcification")]
    CAPSULAR_CALCIFICATION,      // 피막 석회화
    [Description("silicone_invasion_to_capsule")]
    SILICONE_INVASION_TO_CAPSULE,  // 실리콘 피막 침범
    [Description("silicone_invasion_to_ln")]
    SILICONE_INVASION_TO_LN        // 실리콘 임파선 침범
};

