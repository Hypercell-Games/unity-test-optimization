public enum EPlayerPrefsKey
{
    NONE,

    #region APP

    app_user_id,

    app_user_consent,

    #region RATE US

    app_rate_us_success,
    app_rate_us_level_next,

    #endregion RATE US

    #endregion APP

    #region ADS

    ads_i_number,
    ads_r_number,

    #endregion ADS

    #region DATA

    app_remote_config,

    #endregion DATA

    #region GAME

    game_level_completed,

    game_levels_completed,
    game_levels_started,
    game_user_session_number,

    #endregion GAME

    #region SETTINGS

    settings_vibration_enable,
    settings_sounds_enable,
    settings_music_enable,

    #endregion SETTINGS

    #region PLAYER

    player_money,

    player_level_current,

    #endregion PLAYER

    rate_app_shown,
    settings_notifications_enable
}
