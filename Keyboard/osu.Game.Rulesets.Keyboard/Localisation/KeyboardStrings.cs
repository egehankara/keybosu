// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Keyboard.Localisation
{
    /// <summary>
    /// Every piece of text the ruleset shows, in several languages. The language follows osu!lazer's own
    /// language setting and updates live; anything not translated falls back to English.
    /// To add a language, add its two-letter code to <see cref="Languages"/> and a column to every entry below.
    /// </summary>
    public static class KeyboardStrings
    {
        /// <summary>Supported languages, in the order of the columns in <see cref="table"/>.</summary>
        public static readonly string[] Languages = { "en", "tr", "de", "es", "fr", "ja", "ru" };

        // Settings
        public static LocalisableString KeyboardLayout => get(nameof(KeyboardLayout));
        public static LocalisableString KeyboardLayoutTooltip => get(nameof(KeyboardLayoutTooltip));
        public static LocalisableString KeyboardType => get(nameof(KeyboardType));
        public static LocalisableString KeyboardTypeTooltip => get(nameof(KeyboardTypeTooltip));
        public static LocalisableString TypeAnsi => get(nameof(TypeAnsi));
        public static LocalisableString TypeIso => get(nameof(TypeIso));

        // Layout names
        public static LocalisableString LayoutEnglishQwerty => get(nameof(LayoutEnglishQwerty));
        public static LocalisableString LayoutTurkishQ => get(nameof(LayoutTurkishQ));
        public static LocalisableString LayoutTurkishF => get(nameof(LayoutTurkishF));
        public static LocalisableString LayoutGerman => get(nameof(LayoutGerman));
        public static LocalisableString LayoutFrench => get(nameof(LayoutFrench));
        public static LocalisableString LayoutSpanish => get(nameof(LayoutSpanish));
        public static LocalisableString LayoutRussian => get(nameof(LayoutRussian));
        public static LocalisableString LayoutDvorak => get(nameof(LayoutDvorak));
        public static LocalisableString LayoutColemak => get(nameof(LayoutColemak));

        public static LocalisableString HighlightHomeRow => get(nameof(HighlightHomeRow));
        public static LocalisableString HighlightHomeRowTooltip => get(nameof(HighlightHomeRowTooltip));
        public static LocalisableString ScreenCentre => get(nameof(ScreenCentre));
        public static LocalisableString ScreenCentreTooltip => get(nameof(ScreenCentreTooltip));
        public static LocalisableString CentreWholeKeyboard => get(nameof(CentreWholeKeyboard));
        public static LocalisableString CentreSpaceBar => get(nameof(CentreSpaceBar));
        public static LocalisableString CentreBetweenGAndH => get(nameof(CentreBetweenGAndH));

        public static LocalisableString ShowJudgements => get(nameof(ShowJudgements));
        public static LocalisableString ShowJudgementsTooltip => get(nameof(ShowJudgementsTooltip));

        // On the playfield
        public static LocalisableString KeyEnter => get(nameof(KeyEnter));
        public static LocalisableString KeySpace => get(nameof(KeySpace));
        public static LocalisableString HoldSpace => get(nameof(HoldSpace));

        // Columns: en, tr, de, es, fr, ja, ru
        private static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>
        {
            [nameof(KeyboardLayout)] = new[]
            {
                "Keyboard layout", "Klavye düzeni", "Tastaturlayout", "Distribución del teclado", "Disposition du clavier", "キーボード配列", "Раскладка клавиатуры",
            },
            [nameof(KeyboardLayoutTooltip)] = new[]
            {
                "What is printed on the keys. Applies the next time a map starts.",
                "Tuşların üzerinde ne yazdığı. Bir sonraki harita başladığında uygulanır.",
                "Was auf den Tasten steht. Gilt ab der nächsten Map.",
                "Lo que está impreso en las teclas. Se aplica al empezar el siguiente mapa.",
                "Ce qui est imprimé sur les touches. S'applique au début de la prochaine map.",
                "キーに印字されている文字。次の譜面の開始時に適用されます。",
                "Что напечатано на клавишах. Применяется со следующей карты.",
            },
            [nameof(KeyboardType)] = new[]
            {
                "Keyboard type", "Klavye türü", "Tastaturtyp", "Tipo de teclado", "Type de clavier", "キーボードの種類", "Тип клавиатуры",
            },
            [nameof(KeyboardTypeTooltip)] = new[]
            {
                "The shape of your keyboard: flat Enter (ANSI) or tall Enter with an extra key left of Z (ISO). Applies the next time a map starts.",
                "Klavyenin şekli: düz Enter (ANSI) ya da Z'nin solunda ekstra tuşu olan uzun Enter (ISO). Bir sonraki harita başladığında uygulanır.",
                "Die Form deiner Tastatur: flache Enter-Taste (ANSI) oder hohe Enter-Taste mit Zusatztaste links unten (ISO). Gilt ab der nächsten Map.",
                "La forma de tu teclado: Intro plano (ANSI) o Intro alto con una tecla extra a la izquierda de la Z (ISO). Se aplica al empezar el siguiente mapa.",
                "La forme de votre clavier : Entrée plate (ANSI) ou Entrée haute avec une touche en plus en bas à gauche (ISO). S'applique au début de la prochaine map.",
                "キーボードの形状：横長のEnter（ANSI）か、左下に追加キーがある縦長のEnter（ISO）。次の譜面の開始時に適用されます。",
                "Форма клавиатуры: плоский Enter (ANSI) или высокий Enter с дополнительной клавишей слева внизу (ISO). Применяется со следующей карты.",
            },
            [nameof(TypeAnsi)] = new[]
            {
                "ANSI (US) - flat Enter", "ANSI (ABD) - düz Enter", "ANSI (US) - flache Enter-Taste", "ANSI (EE. UU.) - Intro plano", "ANSI (US) - Entrée plate", "ANSI（US）- 横長Enter", "ANSI (США) — плоский Enter",
            },
            [nameof(TypeIso)] = new[]
            {
                "ISO (Turkish / European) - tall Enter", "ISO (Türkçe / Avrupa) - uzun Enter", "ISO (Europa) - hohe Enter-Taste", "ISO (Europa) - Intro alto", "ISO (Europe) - Entrée haute", "ISO（欧州）- 縦長Enter", "ISO (Европа) — высокий Enter",
            },
            [nameof(HighlightHomeRow)] = new[]
            {
                "Highlight home row keys", "Temel sıra tuşlarını vurgula", "Grundreihe hervorheben", "Resaltar la fila guía", "Mettre en évidence la rangée de repos", "ホームポジションを強調", "Подсвечивать основной ряд",
            },
            [nameof(HighlightHomeRowTooltip)] = new[]
            {
                "Makes the eight home row keys where your fingers rest stand out, with bumps on the two index-finger keys like a real keyboard. Applies the next time a map starts.",
                "Parmaklarının durduğu ana sıradaki 8 tuşu belirginleştirir; gerçek klavyedeki gibi iki işaret parmağı tuşunda çıkıntı çizgisi olur. Bir sonraki harita başladığında uygulanır.",
                "Hebt die acht Tasten der Grundreihe hervor, auf denen deine Finger ruhen, mit Markierungen auf den beiden Zeigefinger-Tasten wie bei einer echten Tastatur. Gilt ab der nächsten Map.",
                "Resalta las ocho teclas de la fila guía donde descansan tus dedos, con marcas en las dos teclas de los índices como en un teclado real. Se aplica al empezar el siguiente mapa.",
                "Met en évidence les huit touches de la rangée de repos où reposent vos doigts, avec des repères sur les deux touches des index comme sur un vrai clavier. S'applique au début de la prochaine map.",
                "指を置くホームポジションの8つのキーを目立たせ、実際のキーボードのように人差し指の2つのキーに突起を表示します。次の譜面の開始時に適用されます。",
                "Выделяет восемь клавиш основного ряда, на которых лежат пальцы, с метками на двух клавишах указательных пальцев, как на настоящей клавиатуре. Применяется со следующей карты.",
            },
            [nameof(ShowJudgements)] = new[]
            {
                "Show judgements", "İsabet derecelerini göster", "Bewertungen anzeigen", "Mostrar valoraciones", "Afficher les jugements", "判定を表示", "Показывать оценки попаданий",
            },
            [nameof(ShowJudgementsTooltip)] = new[]
            {
                "Shows Perfect, Great, Miss and so on, small, under the letter of the key you hit. Uses your skin's own judgement graphics. Applies the next time a map starts.",
                "Bastığın tuşun harfinin altında küçükçe Perfect, Great, Miss gibi dereceleri gösterir. Skin'inin kendi derece görsellerini kullanır. Bir sonraki harita başladığında uygulanır.",
                "Zeigt Perfect, Great, Miss usw. klein unter dem Buchstaben der getroffenen Taste. Verwendet die Bewertungsgrafiken deines Skins. Gilt ab der nächsten Map.",
                "Muestra Perfect, Great, Miss, etc. en pequeño bajo la letra de la tecla pulsada. Usa los gráficos de valoración de tu skin. Se aplica al empezar el siguiente mapa.",
                "Affiche Perfect, Great, Miss, etc. en petit sous la lettre de la touche frappée. Utilise les graphismes de jugement de votre skin. S'applique au début de la prochaine map.",
                "押したキーの文字の下に Perfect、Great、Miss などを小さく表示します。スキンの判定画像を使います。次の譜面の開始時に適用されます。",
                "Показывает Perfect, Great, Miss и т. д. мелко под буквой нажатой клавиши. Использует графику оценок из вашего скина. Применяется со следующей карты.",
            },
            [nameof(ScreenCentre)] = new[]
            {
                "Screen centre", "Ekran merkezi", "Bildschirmmitte", "Centro de la pantalla", "Centre de l'écran", "画面の中心", "Центр экрана",
            },
            [nameof(ScreenCentreTooltip)] = new[]
            {
                "Which part of the keyboard sits exactly in the middle of the screen. Applies the next time a map starts.",
                "Klavyenin hangi kısmının ekranın tam ortasında duracağı. Bir sonraki harita başladığında uygulanır.",
                "Welcher Teil der Tastatur genau in der Bildschirmmitte liegt. Gilt ab der nächsten Map.",
                "Qué parte del teclado queda justo en el centro de la pantalla. Se aplica al empezar el siguiente mapa.",
                "Quelle partie du clavier se trouve exactement au centre de l'écran. S'applique au début de la prochaine map.",
                "キーボードのどの部分を画面の中央に置くか。次の譜面の開始時に適用されます。",
                "Какая часть клавиатуры находится точно в центре экрана. Применяется со следующей карты.",
            },
            [nameof(CentreWholeKeyboard)] = new[]
            {
                "Whole keyboard", "Tüm klavye", "Ganze Tastatur", "Todo el teclado", "Tout le clavier", "キーボード全体", "Вся клавиатура",
            },
            [nameof(CentreSpaceBar)] = new[]
            {
                "Space bar", "Boşluk tuşu", "Leertaste", "Barra espaciadora", "Barre d'espace", "スペースキー", "Пробел",
            },
            [nameof(CentreBetweenGAndH)] = new[]
            {
                "Between G and H", "G ile H arası", "Zwischen G und H", "Entre G y H", "Entre G et H", "GとHの間", "Между G и H",
            },
            [nameof(LayoutEnglishQwerty)] = new[]
            {
                "English (QWERTY)", "İngilizce (QWERTY)", "Englisch (QWERTY)", "Inglés (QWERTY)", "Anglais (QWERTY)", "英語（QWERTY）", "Английская (QWERTY)",
            },
            [nameof(LayoutTurkishQ)] = new[]
            {
                "Turkish Q", "Türkçe Q", "Türkisch Q", "Turco Q", "Turc Q", "トルコ語 Q", "Турецкая Q",
            },
            [nameof(LayoutTurkishF)] = new[]
            {
                "Turkish F", "Türkçe F", "Türkisch F", "Turco F", "Turc F", "トルコ語 F", "Турецкая F",
            },
            [nameof(LayoutGerman)] = new[]
            {
                "German (QWERTZ)", "Almanca (QWERTZ)", "Deutsch (QWERTZ)", "Alemán (QWERTZ)", "Allemand (QWERTZ)", "ドイツ語（QWERTZ）", "Немецкая (QWERTZ)",
            },
            [nameof(LayoutFrench)] = new[]
            {
                "French (AZERTY)", "Fransızca (AZERTY)", "Französisch (AZERTY)", "Francés (AZERTY)", "Français (AZERTY)", "フランス語（AZERTY）", "Французская (AZERTY)",
            },
            [nameof(LayoutSpanish)] = new[]
            {
                "Spanish (QWERTY)", "İspanyolca (QWERTY)", "Spanisch (QWERTY)", "Español (QWERTY)", "Espagnol (QWERTY)", "スペイン語（QWERTY）", "Испанская (QWERTY)",
            },
            [nameof(LayoutRussian)] = new[]
            {
                "Russian (ЙЦУКЕН)", "Rusça (ЙЦУКЕН)", "Russisch (ЙЦУКЕН)", "Ruso (ЙЦУКЕН)", "Russe (ЙЦУКЕН)", "ロシア語（ЙЦУКЕН）", "Русская (ЙЦУКЕН)",
            },
            [nameof(LayoutDvorak)] = new[]
            {
                "Dvorak", "Dvorak", "Dvorak", "Dvorak", "Dvorak", "Dvorak", "Дворак",
            },
            [nameof(LayoutColemak)] = new[]
            {
                "Colemak", "Colemak", "Colemak", "Colemak", "Colemak", "Colemak", "Colemak",
            },
            [nameof(KeyEnter)] = new[]
            {
                "Enter", "Enter", "Enter", "Intro", "Entrée", "Enter", "Enter",
            },
            [nameof(KeySpace)] = new[]
            {
                "Space", "Boşluk", "Leertaste", "Espacio", "Espace", "スペース", "Пробел",
            },
            [nameof(HoldSpace)] = new[]
            {
                "Hold Space!", "Boşluk tuşuna basılı tut!", "Leertaste halten!", "¡Mantén Espacio!", "Maintenez Espace !", "スペースを長押し！", "Держите пробел!",
            },
        };

        /// <summary>The text for <paramref name="key"/> in <paramref name="language"/> (a two-letter code), or English.</summary>
        public static string Translate(string key, string? language)
        {
            if (!table.TryGetValue(key, out string[]? row))
                return key;

            int column = language == null ? 0 : Array.IndexOf(Languages, language.ToLowerInvariant());
            return column > 0 && column < row.Length ? row[column] : row[0];
        }

        /// <summary>Every key in the table, for tests.</summary>
        public static IEnumerable<string> AllKeys => table.Keys;

        /// <summary>The raw row for a key, for tests.</summary>
        public static IReadOnlyList<string> RowFor(string key) => table[key];

        private static LocalisableString get(string key) => new LocalisableString(new KeyboardLocalisedString(key));
    }

    /// <summary>
    /// A string that osu!framework re-resolves whenever the game's language changes.
    /// </summary>
    public class KeyboardLocalisedString : ILocalisableStringData
    {
        public string Key { get; }

        public KeyboardLocalisedString(string key)
        {
            Key = key;
        }

        public string GetLocalised(LocalisationParameters parameters)
            => KeyboardStrings.Translate(Key, parameters.Store?.EffectiveCulture.TwoLetterISOLanguageName);

        public bool Equals(ILocalisableStringData? other) => other is KeyboardLocalisedString s && s.Key == Key;

        public override string ToString() => KeyboardStrings.Translate(Key, null);
    }
}
