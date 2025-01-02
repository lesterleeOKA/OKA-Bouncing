using SimpleJSON;
using System;

[Serializable]
public class GameSettings : Settings
{
    public int playerNumber = 0;
    public string frameImageUrl_P1;
    public string frameImageUrl_P2;
    public string grid_image;
    //public string normal_color;
    //public string pressed_color;
}

public static class SetParams
{
    public static void setCustomParameters(GameSettings settings = null, JSONNode jsonNode= null)
    {
        if (settings != null && jsonNode != null)
        {
            ////////Game Customization params/////////
            string frameImageUrl_P1 = jsonNode["setting"]["frame_p1"] != null ?
                jsonNode["setting"]["frame_p1"].ToString().Replace("\"", "") : null;

            string frameImageUrl_P2 = jsonNode["setting"]["frame_p2"] != null ?
                jsonNode["setting"]["frame_p2"].ToString().Replace("\"", "") : null;

            string grid_image = jsonNode["setting"]["grid_image"] != null ?
                jsonNode["setting"]["grid_image"].ToString().Replace("\"", "") : null;

            settings.playerNumber = jsonNode["setting"]["player_number"] != null ? jsonNode["setting"]["player_number"] : null;

            LoaderConfig.Instance.gameSetup.playerNumber = settings.playerNumber;

            /*this.settings.normal_color = jsonNode["setting"]["normal_color"] != null ?
                jsonNode["setting"]["normal_color"].ToString().Replace("\"", "") : null;

            this.settings.pressed_color = jsonNode["setting"]["pressed_color"] != null ? 
                jsonNode["setting"]["pressed_color"].ToString().Replace("\"", "") : null;*/

            /*LoaderConfig.Instance.gameSetup.gridNormalColor = ColorUtility.TryParseHtmlString(settings.normal_color, out Color normalColor) ? normalColor : Color.clear;

            LoaderConfig.Instance.gameSetup.gridPressedColor = ColorUtility.TryParseHtmlString(settings.pressed_color, out Color pressedColor) ? pressedColor : Color.clear;*/

            if (frameImageUrl_P1 != null)
            {
                if (!frameImageUrl_P1.StartsWith("https://") || !frameImageUrl_P1.StartsWith(APIConstant.blobServerRelativePath))
                    settings.frameImageUrl_P1 = APIConstant.blobServerRelativePath + frameImageUrl_P1;
            }

            if (frameImageUrl_P2 != null)
            {
                if (!frameImageUrl_P2.StartsWith("https://") || !frameImageUrl_P2.StartsWith(APIConstant.blobServerRelativePath))
                    settings.frameImageUrl_P2 = APIConstant.blobServerRelativePath + frameImageUrl_P2;
            }

            if (grid_image != null)
            {
                if (!grid_image.StartsWith("https://") || !grid_image.StartsWith(APIConstant.blobServerRelativePath))
                    settings.grid_image = APIConstant.blobServerRelativePath + grid_image;
            }
        }
    }
}

