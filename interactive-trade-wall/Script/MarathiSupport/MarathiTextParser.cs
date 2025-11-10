using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//https://www.unicode.org/charts/PDF/U0900.pdf
//https://www.pramukhfontconverter.com/marathi
public class MarathiTextParser:MonoBehaviour {
    [SerializeField] private InputField _inputField;

    [SerializeField] MarathiToVakraReader _reader;
    //private string marathiText = "महत्त्वाचे";
    private string marathiText = "राज्यातील जनतेला घरांची लॉटरी लागणार आहे. आज झालेल्या मंत्रिमंडळ बैठकीत राज्याच्या नवीन गृहनिर्माण धोरणाला मंजूरी देण्यात आली आहे. ‘माझे घर-माझे अधिकार’ या संकल्पना प्रत्यक्षात राबवण्यात येणार आहे. त्यासाठी 70 हजार कोटींची गुंतवणूक करण्यात येत आहे. त्यानुसार, येत्या 5 वर्षांत 35 लाख घरे उभारण्यात येणार आहेत. EWS, LIG आणि MIG घटकांना घरं देण्याचे उद्दिष्ट डोळ्यासमोर ठेवण्यात आले आहे. मंत्रिमंडळ बैठकीनंतर मुख्यमंत्री देवेंद्र फडणवीस यांनी याविषयीची माहिती दिली.";

    [SerializeField] private Text _text;

    public void UpdateText(string vakraMessage) {
        _text.text = vakraMessage;
    }

    void Start()
    {
        //_inputField.onSubmit.AddListener(OnSubmit);
    }

    public string GetMarathiText(string input)
    {
        string marathiText = string.Empty;
        StringBuilder stringBuilder = new StringBuilder();
        //Debug.Log($"Original Text: {marathiText}");
        //Debug.Log($"Naive Length: {marathiText.Length}");
        List<KVP> _builder = new List<KVP>();

        List<string> characters = SplitMarathiCharacters(input);

        foreach (string unicode in characters) {
            string _vakra = _reader.GetVakra(unicode);

            if (!string.IsNullOrWhiteSpace(unicode) && string.IsNullOrWhiteSpace(_vakra)) {
                foreach (char c in unicode) {
                    string _unicode = SplitGetMarathiCharacters(c.ToString());
                    _vakra = _reader.GetVakra(_unicode);
                    _builder.Add(new KVP() {
                        unicode = _unicode,
                        vakra = _vakra
                    });
                }
                continue;
            }

            //Debug.Log($"{unicode}={_vakra}");
            _builder.Add(new KVP() {
                unicode = unicode,
                vakra = _vakra
            });
        }

        if (IsContainsLeftVowelSign(_builder)) {
            RearrangeList(_builder);
        }

        stringBuilder.Clear();
        foreach (KVP i in _builder) {
            stringBuilder.Append(i.vakra);
        }

        string vakraText = stringBuilder.ToString();

        //Debug.Log($"Mapped Text: {vakraText}");

        //UpdateText(vakraText);
        marathiText = vakraText;
        return marathiText;
    }

    private void OnSubmit(string input) {
        StringBuilder stringBuilder = new StringBuilder();
        //Debug.Log($"Original Text: {marathiText}");
        //Debug.Log($"Naive Length: {marathiText.Length}");
        List<KVP> _builder = new List<KVP>();

        List<string> characters = SplitMarathiCharacters(input);

        foreach (string unicode in characters) {
            string _vakra = _reader.GetVakra(unicode);

            if (!string.IsNullOrWhiteSpace(unicode) && string.IsNullOrWhiteSpace(_vakra)) {
                foreach (char c in unicode) {
                    string _unicode = SplitGetMarathiCharacters(c.ToString());
                    _vakra = _reader.GetVakra(_unicode);
                    _builder.Add(new KVP() {
                        unicode = _unicode,
                        vakra = _vakra
                    });
                }
                continue;
            }

            //Debug.Log($"{unicode}={_vakra}");
            _builder.Add(new KVP() {
                unicode = unicode,
                vakra = _vakra
            });
        }

        if (IsContainsLeftVowelSign(_builder)) {
            RearrangeList(_builder);
        }

        stringBuilder.Clear();
        foreach (KVP i in _builder) {
            stringBuilder.Append(i.vakra);
        }

        string vakraText = stringBuilder.ToString();

        //Debug.Log($"Mapped Text: {vakraText}");

        UpdateText(vakraText);
    }

    private bool IsContainsLeftVowelSign(List<KVP> _builder) {
        return _builder.Any(kvp => kvp.unicode.Any(IsLeftVowelSignAvailable));
    }

    private void RearrangeList(List<KVP> kVPs) {
        if (kVPs == null || kVPs.Count < 2)
            return;

        for (int i = 1;i < kVPs.Count;i++) {
            var current = kVPs[i];
            if (!string.IsNullOrEmpty(current.unicode) && IsLeftVowelSignAvailable(current.unicode[0])) {
                // Swap with the previous element
                var temp = kVPs[i - 1];
                kVPs[i - 1] = current;
                kVPs[i] = temp;
            }
        }
    }

    public List<string> SplitMarathiCharacters(string input) {
        List<string> characters = new List<string>();
        int i = 0;

        while (i < input.Length) {
            if (i + 2 < input.Length && IsConjunctConsonant(input,i)) {
                // Handle conjunct consonants like "त्र", "श्र", "क्त"
                characters.Add(input.Substring(i,3));
                i += 3;
            } else if (i + 1 < input.Length && IsVowelSign(input[i + 1])) {
                // Handle base character + vowel sign (e.g., क + ी = की)
                characters.Add(input.Substring(i,2));
                i += 2;
            } else if (i + 1 < input.Length && IsHalant(input[i + 1])) {
                // Handle halant forms (half consonants)
                characters.Add(input.Substring(i,2));
                i += 2;
            } else {
                // Single character (vowel, consonant without sign, or symbol)
                characters.Add(input[i].ToString());
                i++;
            }
        }

        return characters;
    }

    public string SplitGetMarathiCharacters(string input) {
        string characters = string.Empty;
        int i = 0;

        while (i < input.Length) {
            if (i + 2 < input.Length && IsConjunctConsonant(input,i)) {
                // Handle conjunct consonants like "त्र", "श्र", "क्त"
                characters = input.Substring(i,3);
                i += 3;
            } else if (i + 1 < input.Length && IsVowelSign(input[i + 1])) {
                // Handle base character + vowel sign (e.g., क + ी = की)
                characters = input.Substring(i,2);
                i += 2;
            } else if (i + 1 < input.Length && IsHalant(input[i + 1])) {
                // Handle halant forms (half consonants)
                characters = input.Substring(i,2);
                i += 2;
            } else {
                // Single character (vowel, consonant without sign, or symbol)
                characters = input[i].ToString();
                i++;
            }
        }

        return characters;
    }
    private bool IsVowelSign(char c) {
        // Devanagari vowel signs range
        return c >= '\u093E' && c <= '\u094C';
    }

    //https://symbl.cc/en/unicode/blocks/devanagari/
    private bool IsLeftVowelSignAvailable(char c) {
        // Devanagari vowel signs range
        return c == '\u093F' || c == '\u094E';
    }

    private bool IsHalant(char c) {
        // Devanagari halant (virama)
        return c == '\u094D';
    }

    private bool IsConjunctConsonant(string text,int index) {
        // Check bounds
        if (index < 0 || index + 1 >= text.Length)
            return false;

        // Check for explicit conjunct characters (single codepoint)
        char current = text[index];

        // Check for half-form + virama + consonant pattern
        if (index + 2 < text.Length) {
            char c1 = text[index];
            char c2 = text[index + 1];
            char c3 = text[index + 2];

            // Standard pattern: consonant + virama + consonant
            if (IsConsonant(c1) && c2 == '\u094D' && IsConsonant(c3))
                return true;
        }

        return false;
    }

    private bool IsConsonant(char c) {
        // Check if character is in the Devanagari consonant range
        return (c >= '\u0915' && c <= '\u0939') ||
               (c >= '\u0958' && c <= '\u095F') ||
               (c >= '\u097B' && c <= '\u097F');
    }
}

public class KVP {
    public string unicode;
    public string vakra;
}