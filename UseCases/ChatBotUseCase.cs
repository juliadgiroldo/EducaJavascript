using System.Text.Json;

namespace ChatbotEducacionalApi.UseCases
{
    public class DialogflowUseCase
    {
        private static readonly Dictionary<string, StatusModeracao> _bancoModeracao = new();

        private static readonly string[] Blacklist = { "merda", "lixo", "foder", "fuder", "caralho", "bosta", "inutil", "pqp", "cocô", "puta que pariu", "porra", "prr", "crlh", "burro", "imbecil" };

        // ================== PERGUNTAS ==================

        private static readonly List<PerguntaQuiz> ListaQuiz = new()
        {
            new PerguntaQuiz { Id = 1, Texto = "Como mostrar uma mensagem no terminal?", Opcoes = "1 - echo\n2 - console.log\n3 - print", Correta = "2", Feedback = "O console.log é a função nativa do JS." },
            new PerguntaQuiz { Id = 2, Texto = "Variável que NÃO pode ser alterada?", Opcoes = "1 - let\n2 - var\n3 - const", Correta = "3", Feedback = "A palavra 'const' cria uma constante." },
            new PerguntaQuiz { Id = 3, Texto = "Comentário de uma linha?", Opcoes = "1 - //\n2 - ##\n3 - /*", Correta = "1", Feedback = "O // comenta a linha atual." },
            new PerguntaQuiz { Id = 4, Texto = "Transformar string em inteiro?", Opcoes = "1 - parseInteger()\n2 - parseInt()\n3 - toInt()", Correta = "2", Feedback = "parseInt() analisa a string." },
            new PerguntaQuiz { Id = 5, Texto = "Propriedade para tamanho de array?", Opcoes = "1 - length\n2 - size\n3 - count", Correta = "1", Feedback = "A propriedade length retorna o número de itens." },
            new PerguntaQuiz { Id = 6, Texto = "Operador 'Estritamente Igual'?", Opcoes = "1 - ==\n2 - ===\n3 - =", Correta = "2", Feedback = "=== verifica valor e tipo." },
            new PerguntaQuiz { Id = 7, Texto = "Laço que executa pelo menos uma vez?", Opcoes = "1 - for\n2 - while\n3 - do...while", Correta = "3", Feedback = "do...while valida a condição no fim." },
            new PerguntaQuiz { Id = 8, Texto = "Adicionar item no final do array?", Opcoes = "1 - push()\n2 - pop()\n3 - shift()", Correta = "1", Feedback = "push() insere no final." },
            new PerguntaQuiz { Id = 9, Texto = "Tipo de dado de 'true'?", Opcoes = "1 - string\n2 - boolean\n3 - number", Correta = "2", Feedback = "Valores lógicos são booleanos." },
            new PerguntaQuiz { Id = 10, Texto = "Como chamar a função 'teste'?", Opcoes = "1 - teste()\n2 - call teste\n3 - run teste", Correta = "1", Feedback = "Usamos parênteses para invocar." }
        };

        private static readonly List<PerguntaQuiz> ListaQuestionario = new()
        {
            new PerguntaQuiz { Id = 1, Texto = "Método que converte JSON em objeto?", Opcoes = "1 - parse\n2 - stringify\n3 - toObject\n4 - convert", Correta = "1" },
            new PerguntaQuiz { Id = 2, Texto = "Comando que cria escopo de bloco?", Opcoes = "1 - var\n2 - let\n3 - function\n4 - static", Correta = "2" },
            new PerguntaQuiz { Id = 3, Texto = "Símbolo que representa um Array?", Opcoes = "1 - {}\n2 - ()\n3 - []\n4 - <>", Correta = "3" },
            new PerguntaQuiz { Id = 4, Texto = "Remove o último item de um array?", Opcoes = "1 - shift\n2 - delete\n3 - pop\n4 - remove", Correta = "3" },
            new PerguntaQuiz { Id = 5, Texto = "Operador lógico AND?", Opcoes = "1 - ||\n2 - &&\n3 - !\n4 - &", Correta = "2" },
            new PerguntaQuiz { Id = 6, Texto = "Formato de uma Arrow Function?", Opcoes = "1 - function()\n2 - () => {}\n3 - func {}\n4 - => ()", Correta = "2" },
            new PerguntaQuiz { Id = 7, Texto = "Qual destes NÃO é um tipo em JS?", Opcoes = "1 - string\n2 - number\n3 - float\n4 - undefined", Correta = "3" },
            new PerguntaQuiz { Id = 8, Texto = "Método que percorre o array?", Opcoes = "1 - forEach\n2 - loop\n3 - iterate\n4 - run", Correta = "1" },
            new PerguntaQuiz { Id = 9, Texto = "O que retorna typeof null?", Opcoes = "1 - null\n2 - undefined\n3 - object\n4 - string", Correta = "3" },
            new PerguntaQuiz { Id = 10, Texto = "Estrutura para tratar erros?", Opcoes = "1 - if/else\n2 - try/catch\n3 - switch\n4 - for", Correta = "2" }
        };

        // ================== ENTRADA PRINCIPAL ==================

        public object Executar(JsonElement payload)
        {
            var queryResult = payload.GetProperty("queryResult");
            var session = payload.GetProperty("session").GetString() ?? "";
            string textoUsuario = queryResult.GetProperty("queryText").GetString()?.Trim().ToLower() ?? "";
            var intentName = queryResult.GetProperty("intent").GetProperty("displayName").GetString();

            if (!_bancoModeracao.ContainsKey(session))
                _bancoModeracao[session] = new StatusModeracao();

            var status = _bancoModeracao[session];

            if (status.BloqueioDefinitivo)
                return RespostaSimples("🚫 Acesso Negado. Você foi bloqueado.");

            if (status.BloqueadoAte.HasValue && status.BloqueadoAte > DateTime.Now)
            {
                var rest = Math.Ceiling((status.BloqueadoAte.Value - DateTime.Now).TotalMinutes);
                return RespostaSimples($"⏳ Suspenso por {rest} minuto(s).");
            }

            bool eInadequado = Blacklist.Any(p => textoUsuario.Contains(p))
                               || intentName == "Default Fallback Intent"
                               || intentName == "mensagens_inadequadas";

            if (eInadequado)
                return AplicarPunicao(status, session);

            if (textoUsuario == "0")
                return ResetarContextos(session);

            // ===== QUIZ =====
            if (intentName == "exercicio")
                return IniciarAtividade(session, ListaQuiz, "fazendo_quiz", "🚀 QUIZ JS");

            if (intentName == "Responder_Exercicio")
                return ProcessarAtividade(payload, session, textoUsuario, ListaQuiz, "fazendo_quiz", true);

            // ===== QUESTIONÁRIO =====
            if (intentName == "questionario")
                return IniciarAtividade(session, ListaQuestionario, "fazendo_questionario", "📝 QUESTIONÁRIO");

            if (intentName == "responder_questionario")
                return ProcessarAtividade(payload, session, textoUsuario, ListaQuestionario, "fazendo_questionario", false);

            return RespostaSimples("Digite uma opção válida.");
        }

        // ================== FLUXO ATIVIDADES ==================

        private object IniciarAtividade(string session, List<PerguntaQuiz> lista, string contexto, string titulo)
        {
            var p = lista.First();
            return MontarResposta(session, $"{titulo} (1/10)\n\n{p.Texto}\n{p.Opcoes}", 1, 0, contexto);
        }

        private object ProcessarAtividade(JsonElement payload, string session, string resposta, List<PerguntaQuiz> lista, string contexto, bool comFeed)
        {
            var queryResult = payload.GetProperty("queryResult");
            int idAtual = 1; int pontos = 0;

            if (queryResult.TryGetProperty("outputContexts", out JsonElement contexts))
            {
                foreach (var c in contexts.EnumerateArray())
                {
                    if (c.GetProperty("name").GetString().Contains(contexto))
                    {
                        var paras = c.GetProperty("parameters");
                        idAtual = ExtrairInt(paras, "id_pergunta", 1);
                        pontos = ExtrairInt(paras, "pontuacao_atual", 0);
                    }
                }
            }

            var pAtual = lista.First(p => p.Id == idAtual);
            bool acertou = (resposta == pAtual.Correta);
            if (acertou) pontos += comFeed ? 10 : 1;

            var proxima = lista.FirstOrDefault(p => p.Id == idAtual + 1);

            if (proxima != null)
            {
                string msg = "";
                if (comFeed)
                    msg = acertou
                        ? $"✅ Correto! {pAtual.Feedback}\n\n"
                        : $"❌ Errado! Era {pAtual.Correta}. {pAtual.Feedback}\n\n";

                msg += $"Próxima ({proxima.Id}/10):\n{proxima.Texto}\n{proxima.Opcoes}";

                return MontarResposta(session, msg, proxima.Id, pontos, contexto);
            }

            string fim = comFeed
                ? $"🏆 QUIZ CONCLUÍDO! Nota Final: {pontos}/100"
                : $"📊 QUESTIONÁRIO FINALIZADO!\n✅ Acertos: {pontos}\n❌ Erros: {10 - pontos}";

            return MontarResposta(session, fim, 0, 0, "menu", 2);
        }

        // ================== MODERAÇÃO ==================

        private object AplicarPunicao(StatusModeracao status, string session)
        {
            status.ContadorAvisos++;

            if (status.ContadorAvisos == 2)
                status.BloqueadoAte = DateTime.Now.AddMinutes(5);

            if (status.ContadorAvisos >= 3)
                status.BloqueioDefinitivo = true;

            return RespostaSimples("⚠️ Linguagem inadequada detectada.");
        }

        // ================== HELPERS ==================

        private object ResetarContextos(string session)
        {
            return new
            {
                fulfillmentText = "🔄 Atividade encerrada.",
                outputContexts = new[] {
                    new { name = $"{session}/contexts/fazendo_quiz", lifespanCount = 0 },
                    new { name = $"{session}/contexts/fazendo_questionario", lifespanCount = 0 }
                },
                endInteraction = true
            };
        }

        private object RespostaSimples(string texto)
            => new { fulfillmentText = texto };

        private int ExtrairInt(JsonElement el, string prop, int padrao)
        {
            if (el.TryGetProperty(prop, out JsonElement p))
            {
                if (p.ValueKind == JsonValueKind.Number) return (int)p.GetDouble();
                if (p.ValueKind == JsonValueKind.String && double.TryParse(p.GetString(), out double r)) return (int)r;
            }
            return padrao;
        }

        private object MontarResposta(string session, string texto, int id, int pontos, string contexto, int life = 5)
        {
            return new
            {
                fulfillmentMessages = new[] { new { text = new { text = new[] { texto } } } },
                outputContexts = new[] {
                    new {
                        name = $"{session}/contexts/{contexto}",
                        lifespanCount = life,
                        parameters = new Dictionary<string, object> {
                            { "id_pergunta", id },
                            { "pontuacao_atual", pontos }
                        }
                    }
                }
            };
        }
    }
}