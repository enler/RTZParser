using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTZParser
{
    internal class RTZDefaultType
    {
        private static Dictionary<UInt32, string> DefaultTypeTable = new Dictionary<uint, string>()
        {
            {0x13082CF8,"#null_type"},
            {0x5F35F444,"#undefined"},
            {0x1B9338B9,"#class"},
            {0x67D1175F,"#delegate"},
            {0x66BA25FA,"auto"},
            {0x6DD08706,"var"},
            {0xB97A05D2,"WeakReference"},
            {0x1451DAB1,"int"},
            {0xC9A55E95,"float"},
            {0x9A797C98,"float2"},
            {0xED7E4C0E,"float3"},
            {0x731AD9AD,"float4"},
            {0x9EBEB2A9,"string"},
            {0x9BDF22D1,"wstring"},
            {0x55813692,"bool"},
            {0xD27BD9EE,"void"},
            {0x6AB9363A,"voidptr"},
            {0xA10CEEB7,"array"},
            {0x07EE7E78,"assoc_array"},
            {0xA2475F62,"iassoc_array"},
            {0xD0D3C013,"fassoc_array"},
            {0x0C6B5B37,"byteptr"},
            {0xB19943CE,"bytes"},
            {0x2F55A231,"preload_obj_t"},
            {0x7DE7323A,"script_t"},
            {0xEF3ECF65,"strings_t"},
            {0x511F9F86,"uad_t"},
            {0x11DCA6FE,"image_t"},
            {0xEB2D8637,"sprites_t"},
            {0xBD6438B6,"textcanvas_t"},
            {0xED8DF6F4,"canvas_t"},
            {0x7DA39233,"drawing_context_t"},
            {0xE6521190,"render_stage_t"},
            {0x97B5461F,"scissor_box_t"},
            {0x06F41261,"g3dres_t"},
            {0xC5635F92,"model_t"},
            {0xFB6A3192,"anime_t"},
            {0x60F0ED44,"scene_t"},
            {0x1032922B,"bone_t"},
            {0xAF685C94,"gizmo_t"},
            {0x527F0322,"skybox_t"},
            {0xBDCEFFE0,"BulletGroundInfo"},
            {0x8A38225C,"BulletCapsuleSensor"},
            {0xA90A538A,"rbinfo_t"},
            {0x107493B0,"ease_t"},
            {0x661F1800,"saveload_slot_info_t"},
            {0xC40610C6,"saveload_t"}
        };

        public static string GetName(UInt32 hash)
        {
            if (DefaultTypeTable.ContainsKey(hash))
                return DefaultTypeTable[hash];
            else
                return "";
        }
    }
}
