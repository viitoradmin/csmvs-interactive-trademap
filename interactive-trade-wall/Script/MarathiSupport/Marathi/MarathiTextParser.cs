using System.Collections.Generic;
using System.Text;
using UnityEngine;

//https://www.unicode.org/charts/PDF/U0900.pdf
//https://www.pramukhfontconverter.com/marathi
[RequireComponent(typeof(MarathiToVakraReader))]
public class MarathiTextParser : MonoBehaviour {
    [SerializeField] private MarathiToVakraReader _reader;

    // Hardcoded overrides for special Vakra characters or ligatures
    private readonly Dictionary<string, string> _specialLigatures = new Dictionary<string, string> {
        { "\u091F\u094D\u0930", "¨" }, // Tra (ट+्+र) -> ¨ (Used in Trap)
        { "\u091F\u094D\u092F", "«" }, // Tya (ट+्+य) -> « (Used in Bibatya)
        { "\u0945", "š" },             // Chandra Bindu Matra -> š
        
        // Special Case: "Constantinople" -> nti -> œN3
        // Na(0928) + Vir + Ta(091F) + Velanti(093F)
        { "\u0928\u094D\u091F\u093F", "œN3" } 
    };

    // Single Character Overrides to ensure correct mapping even if JSON is missing them
    private readonly Dictionary<char, string> _charOverrides = new Dictionary<char, string> {
        { '\u0923', "`" }, // Na (Baan) -> ` (Used in Karnabhushan)
        { '\u092D', "w" }, // Bha -> w (Used in Karnabhushan)
        { '\u0937', "8" }, // Sha -> 8 (Used in Karnabhushan)
        { '\u0942', "U" }, // UU Matra -> U
        { '\u091F', "3" }, // Ta -> 3 (Used in Constantinople/Garnet)
        { ';', "å" }       // Semicolon -> å (Used in Turkey)
    };

    private void Awake() {
        if (_reader == null) {
            _reader = GetComponent<MarathiToVakraReader>();
        }
    }

    internal string GetMarathiText(string input) {
        if (string.IsNullOrEmpty(input))
            return "";

        // 1. Clean Text
        input = input.Replace("\r", "").Replace("\n", "").Replace("\u200B", "");

        StringBuilder sb = new StringBuilder();
        int i = 0;
        int len = input.Length;

        while (i < len) {
            char currentChar = input[i];

            // --- PRIORITY 1: CHECK 4-CHAR SPECIAL LIGATURES ---
            // Example: 'nti' in Constantinople (Na+Vir+Ta+Velanti)
            if (i + 3 < len) {
                string fourCharKey = input.Substring(i, 4);
                if (_specialLigatures.ContainsKey(fourCharKey)) {
                    sb.Append(_specialLigatures[fourCharKey]);
                    i += 4;
                    continue;
                }
            }

            // --- PRIORITY 2: CHECK 3-CHAR SPECIAL LIGATURES ---
            // Example: Tra (¨), Tya («), Dya
            if (i + 2 < len) {
                string threeCharKey = input.Substring(i, 3);
                
                // Specific Check for 'Dya' (D + Vir + Y) -> D + special-ya
                if (threeCharKey == "\u0921\u094D\u092F") 
                {
                    string baseDa = GetMap("\u0921"); // D
                    sb.Append(baseDa);
                    sb.Append(""); // Special Vakra symbol for half-ya
                    i += 3;
                    continue;
                }

                if (_specialLigatures.ContainsKey(threeCharKey)) {
                    sb.Append(_specialLigatures[threeCharKey]);
                    i += 3;
                    continue;
                }
            }

            // --- PRIORITY 3: CHECK SPECIAL HALF-FORMS (2 chars) ---
            // Example: Maharashtra 'Shta' logic -> Sh + Vir before T
            if (i + 1 < len) {
                if (input[i] == '\u0937' && input[i + 1] == '\u094D') {
                    // Peek ahead to see if next is T (or T+Vir+R)
                    if (i + 2 < len && input[i + 2] == '\u091F') {
                        sb.Append("*");
                        i += 2; 
                        continue;
                    }
                }
            }

            // --- PRIORITY 4: REPH (Ra + Virama + Consonant) ---
            // Logic: Ra + Vir + Cons -> Cons + Reph
            // UPDATED Logic: Ra + Vir + Cons + [Matra] -> Cons + [Matra] + Reph
            // Example: Garnet (neR), Turkey (kIR), Karnabhushan (NaR)
            if (i + 2 < len &&
                input[i] == '\u0930' &&
                input[i + 1] == '\u094D' &&
                IsConsonant(input[i + 2])) {

                // Check if Consonant is followed by a Matra (Velanti, Matra, etc.)
                // This handles Turkey (k + I + R) and Garnet (n + e + R)
                if (i + 3 < len && IsMatra(input[i + 3])) {
                    // Get Consonant + Matra map (e.g., 'ki', 'ne')
                    string consKey = input[i + 2].ToString();
                    string matraKey = input[i + 3].ToString();
                    string fullKey = consKey + matraKey;
                    
                    // Try combined map first (e.g. 'ki' -> 'ik')
                    string combinedMap = _reader.GetVakra(fullKey);
                    
                    if (string.IsNullOrEmpty(combinedMap)) {
                        // Build manually if combined map missing
                        string cMap = GetMap(consKey);
                        string mMap = GetMap(matraKey);
                        // Standard Velanti logic (Velanti before Consonant) is handled by 'GetMap' if defined,
                        // otherwise we append in order. For Vakra, 'ik' implies logic is in map.
                        combinedMap = cMap + mMap; 
                    }

                    string reph = GetMap("Z"); // Reph map (Usually Z or R)
                    if (string.IsNullOrEmpty(reph) || reph == "Z") reph = "R"; // Fallback to R based on 'Garnet'

                    sb.Append(combinedMap + reph);
                    i += 4; // Skip Ra, Vir, Cons, Matra
                    continue;
                }
                
                // Normal Reph (No Matra) - Karnabhushan (Na + Reph)
                string consNormal = GetMap(input[i + 2].ToString());
                string rephNormal = GetMap("Z");
                if (string.IsNullOrEmpty(rephNormal) || rephNormal == "Z") rephNormal = "R"; 

                sb.Append(consNormal + rephNormal);
                i += 3;
                continue;
            }

            // --- PRIORITY 5: RA-KAR (Consonant + Virama + Ra) ---
            // Example: 'Bri' (British) -> b + vir + r + i
            // Logic: Cons + Vir + Ra + Velanti -> Velanti + Cons + RaKar
            if (i + 3 < len &&
                IsConsonant(input[i]) &&
                input[i + 1] == '\u094D' &&
                input[i + 2] == '\u0930' &&
                input[i + 3] == '\u093F') // Velanti
            {
                string cons = GetMap(input[i].ToString());
                string raKar = "/"; 
                string velanti = GetMap("\u093F");
                if(string.IsNullOrEmpty(velanti)) velanti = "i"; // Fallback from British example

                sb.Append(velanti + cons + raKar);
                i += 4;
                continue;
            }

            if (i + 2 < len &&
                IsConsonant(input[i]) &&
                input[i + 1] == '\u094D' &&
                input[i + 2] == '\u0930') 
            {
                string cons = GetMap(input[i].ToString());
                string raKar = "/"; 
                sb.Append(cons + raKar);
                i += 3;
                continue;
            }

            // --- PRIORITY 6: VELANTI (Consonant + Velanti) ---
            // Example: 'ki' -> 'ik'
            if (i + 1 < len && IsConsonant(input[i]) && input[i + 1] == '\u093F') {
                string cons = GetMap(input[i].ToString());
                string velanti = GetMap("\u093F");
                sb.Append(velanti + cons);
                i += 2;
                continue;
            }

            // --- PRIORITY 7: HALF CONSONANTS (Consonant + Virama + Consonant) ---
            if (i + 1 < len && IsConsonant(input[i]) && input[i + 1] == '\u094D') {
                string baseKey = input[i].ToString();
                string halfMap = _reader.GetVakra(baseKey + "्"); 

                if (string.IsNullOrEmpty(halfMap)) {
                    // Heuristic: Uppercase the base map
                    string baseMap = GetMap(baseKey);
                    if (!string.IsNullOrEmpty(baseMap) && baseMap.Length == 1) {
                        halfMap = baseMap.ToUpper();
                    } else {
                        halfMap = baseMap; 
                    }
                }

                sb.Append(halfMap);
                i += 2; 
                continue;
            }

            // --- PRIORITY 8: SINGLE CHAR / MATRA ---
            string charKey = input[i].ToString();
            
            // Check overrides first
            if (_specialLigatures.ContainsKey(charKey)) {
                sb.Append(_specialLigatures[charKey]);
            }
            else {
                string mapped = GetMap(charKey); // Use helper for overrides
                sb.Append(string.IsNullOrEmpty(mapped) ? charKey : mapped);
            }
            i++;
        }

        return sb.ToString();
    }

    // Helper to get map with fallback and overrides
    private string GetMap(string key) {
        // 1. Check Hardcoded Char Overrides first
        if (key.Length == 1 && _charOverrides.ContainsKey(key[0])) {
            return _charOverrides[key[0]];
        }

        // 2. Check JSON
        string val = _reader.GetVakra(key);
        
        // 3. Fallbacks if JSON missing
        if (string.IsNullOrEmpty(val)) {
            if (key == "Z") return "Z";
            if (key == "\u093F") return "f"; // Default Velanti fallback
            if (key == "\u0940") return "I"; // Default Dirgha Velanti fallback
        }
        return val;
    }

    private bool IsConsonant(char c) {
        return (c >= '\u0915' && c <= '\u0939') ||
               (c >= '\u0958' && c <= '\u095F');
    }

    private bool IsMatra(char c) {
        // Range for Matras (Velanti, Ukar, Matra, etc.)
        // 093A (e.g. vowel sign) to 094C (vowel sign au)
        // Includes 093F (i), 0940 (I), 0941 (u), 0947 (e), etc.
        return (c >= '\u093A' && c <= '\u094C') || 
               c == '\u0962' || c == '\u0963'; // Vowel sign L/LL
    }
}