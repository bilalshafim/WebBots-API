using System.ComponentModel;

namespace Araka_WebBots.Bots.ES.Models
{
    public class ESBotConfig
    {
        [DefaultValue(null)]
        public object JSONServiceAccount { get; set; } = "{\r\n  \"type\": \"service_account\",\r\n  \"project_id\": \"chromatic-craft-384800\",\r\n  \"private_key_id\": \"0ff7e5fdc016a4bd4d943599f9dd6ee3d283d3c6\",\r\n  \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDfxdKZWwMcywcJ\\n3aIXNKS7gu10b60/MRU/Bfs7ThwNmoI+8A6VzB5ClB4ZxsMI4gjuQjPBsrCfJ+CP\\n+P/ZPDQFERaO2WkXz0J6TVMGQ2Qm/WdwMCCTnmZiESJ8agUKTqfQOj/nvyJsV5Od\\nDR3EgKXQ47+O4kXs54tEleUcMVbqArj7ZIbsQ+UmuueFX/Y4TguewnNS1d5N2MYH\\nkblzSX+mKkIgFIkf6RB/5ya5p8xy/2tgGksMLIkE0GZy9quuITzcAL0PN9pPKS0W\\nAy7SnX+L3dzNis+r6fdkPaXPNyrOqM//xjN1SvFnHKjNYRxrGZK3FnvRVQDaIjWC\\nAFBhySBnAgMBAAECggEAGW0fnOVGg+2zLbUinIMSz2ufvOyOQZvmJMnH3i6QlCB2\\nTt3KtSKtsuBO+0bRFR/55nix638jqodGxj/Yx7+beZKTfu/H4o4I/l9C4SH52KRP\\n2I40urvzjjkRiL7TNnScL4DZj2TxwwsVxBv1bJqMPMwOvBbh3dABdxhBIY70UpUo\\nzKSMaz07ZShfQAbqqISO2ozyS7WOvYruv1zCV32ROA1DWvHKuwFfXq0DCqLLhPq7\\nK5DEatkBRdX94b/b1gh4lhyi3g+C2GtXTn84vtG5E+TL/G+25Z7VU+eXpVOLz7Sb\\nEZEMLVJuEhMTjD+ACkORUZXU3nI61cEWFhlW7Qb/oQKBgQD9ZwbJDIHLa15qlPDC\\nFXYLH3A3+cjeF+WQcBrfgznzTRa8nlVKoTh9LTab6axFgMqdUfCvWeDJHAROMrNO\\nwVitunGikEisRYVHzcDkykU2Syg2PXs683Q4N7GoARYIiUzB0T2z0qEHNQVNlmQH\\n+5WQG0U8epjb6xiXpxhNxgFBswKBgQDiEQrgq/OPUD+BR630Nn0sq0ksPrMTcojy\\n9RScGE8N0+0J7EdeUwDkXHxSFAiEP95Z3v+TgT9HH2oa06lAB0RGY/sP3FB19VnV\\nFZ0eU1dTvV4m3TcGE1dY3PznUD4nzA91PZBSlnkehjVqXT/awf1yaVWKXVIUP+WY\\nG45oyiXEfQKBgQDnc7cK49WZy5QNncTaQWf0eN6zAf+qfAE+kGa7SM0hoYPm/GfD\\nJsbF7wo2FpU3Vo+6aiGAyIM8rc5w7nZAtPEenXE1nrwCPe2izBgn4WieUr+D61wn\\nWD8LKjOm1HcfjOkDDu9g1D4oqxEQ8RClCfJuEbqHpyL/nBh2TDUytGxpaQKBgQCN\\nmLEggnN+y3vLZPM/KLYEuZCOhwMxZicDTqDHGK7DcX9iHL2jBghkAM3ZtzSSaVLD\\nJdsdL/JLgRC7luHG+gY4tLz3dT5sc+39epk7+mWTTORhIWQqiQjH3zsFQ4x67uSr\\nwZDZOejJRrTEV338bk9qTzHGBae4iUEsoe30EtHPHQKBgBSC4CRzJsvQSJEaJQHR\\nnZA9S9S4bdSL7je0CPCnXPtFuXOdWKz0QDMc8T5Q9LaZjqedfMtHlxHyCAcWdLPa\\nE0jml0Vpm+kstpzNiLaeQB8kdUqo/vQLcKmrdTGvd716kXuzonk+lvmuqzuUDJdz\\nasqn4gQycb1I4Ag4xBhLGM0t\\n-----END PRIVATE KEY-----\\n\",\r\n  \"client_email\": \"dialogflow-custom-service-acco@chromatic-craft-384800.iam.gserviceaccount.com\",\r\n  \"client_id\": \"108518350911556270456\",\r\n  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\r\n  \"token_uri\": \"https://oauth2.googleapis.com/token\",\r\n  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\r\n  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/dialogflow-custom-service-acco%40chromatic-craft-384800.iam.gserviceaccount.com\"\r\n}";

        [DefaultValue("default")]
        public string GCPProject { get; set; } = "chromatic-craft-384800";

        [DefaultValue("default")]
        public string Region { get; set; } = "global";
        
        [DefaultValue("default")]
        public string Environment { get; set; }
    }
}
