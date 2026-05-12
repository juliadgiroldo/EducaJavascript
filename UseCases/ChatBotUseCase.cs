using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ChatbotEducacionalApi.UseCases
{
    public class DialogflowUseCase
    {
        private static readonly Dictionary<string, StatusModeracao> _bancoModeracao = new();

        private static readonly string[] Blacklist = { "merda", "lixo", "foder", "fuder", "caralho", "bosta", "inutil", "pqp", "cocô", "puta que pariu", "porra", "prr", "crlh", "burro", "imbecil" };

        private const string TextoMenu = 
            "Escolha uma das opções abaixo para continuarmos:\n\n" +
            "0 - Finalizar a conversa\n" +
            "1 – Conteúdo explicativo sobre JavaScript\n" +
            "2 – Resolução de exercícios\n" +
            "3 – Questionário para praticar";

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
            
            string textoUsuario = queryResult.TryGetProperty("queryText", out JsonElement qt) ? qt.GetString()?.Trim().ToLower() ?? "" : "";
            string intentName = "";
            
            if (queryResult.TryGetProperty("intent", out JsonElement intentObj) && intentObj.TryGetProperty("displayName", out JsonElement dispNameObj))
                intentName = dispNameObj.GetString() ?? "";

            // --- Moderação ---
            if (!_bancoModeracao.ContainsKey(session)) _bancoModeracao[session] = new StatusModeracao();
            var status = _bancoModeracao[session];

            if (status.BloqueioDefinitivo) return RespostaSimples("🚫 Acesso Negado. Você foi bloqueado.");
            if (status.BloqueadoAte.HasValue && status.BloqueadoAte > DateTime.Now)
            {
                var rest = Math.Ceiling((status.BloqueadoAte.Value - DateTime.Now).TotalMinutes);
                return RespostaSimples($"⏳ Suspenso por {rest} minuto(s).");
            }

            bool eInadequado = Blacklist.Any(p => textoUsuario.Contains(p)) || intentName == "mensagens_inadequadas";
            if (eInadequado) return AplicarPunicao(status, session);

            // --- Saída Global ---
            if (textoUsuario == "0" || textoUsuario == "sair")
                return ResetarContextos(session);

            // --- ESTADOS DE CONVERSA (Contextos) ---
            bool emQuiz = EstaEmContexto(queryResult, "fazendo_quiz");
            bool emQuestionario = EstaEmContexto(queryResult, "fazendo_questionario");
            bool aguardandoCurso = EstaEmContexto(queryResult, "aguardando_escolha_curso");

            // 1. Processar Quiz ou Questionário
            if (emQuiz) return ProcessarAtividade(payload, session, textoUsuario, ListaQuiz, "fazendo_quiz", true);
            if (emQuestionario) return ProcessarAtividade(payload, session, textoUsuario, ListaQuestionario, "fazendo_questionario", false);

            // 2. Processar Submenu de Conteúdo (Se o usuário apertou 1 antes)
            if (aguardandoCurso)
            {
                string respostaConteudo = "";
                if (textoUsuario == "1" || intentName == "OQueEJavascript") 
                    respostaConteudo = "📚 Acesse a introdução ao JavaScript no link: [link introdução]";
                else if (textoUsuario == "2" || intentName == "ConceitosFundamentais") 
                    respostaConteudo = "📚 Acesse os Conceitos Fundamentais no link: [link conceitos]";
                else if (textoUsuario == "3" || intentName == "Funcoes") 
                    respostaConteudo = "📚 Acesse o material sobre Funções no link: [link funções]";
                else 
                    return RespostaSimples("Opção inválida. Escolha 1, 2 ou 3 para os conteúdos, ou digite 0 para voltar.");

                // Devolve a matéria e exibe o menu principal de volta
                return new
                {
                    fulfillmentMessages = new[] { new { text = new { text = new[] { $"{respostaConteudo}\n\n{TextoMenu}" } } } },
                    outputContexts = new[] {
                        new { name = $"{session}/contexts/aguardando_escolha_curso", lifespanCount = 0, parameters = new {} },
                        new { name = $"{session}/contexts/esperando_menu_principal", lifespanCount = 5, parameters = new {} }
                    }
                };
            }

            // 3. Menu Principal (Se não está em nenhuma das atividades acima)
            if (intentName == "Default Welcome Intent" || textoUsuario == "menu" || textoUsuario == "oi" || textoUsuario == "ola")
                return RespostaComMenu(session, "Olá! " + TextoMenu);

            // CORREÇÃO: A linha abaixo estava faltando. Leva o usuário para o Submenu de Estudo e ativa o contexto.
            if (intentName == "ConteudoExplicativo" || textoUsuario == "1")
            {
                string subMenu = "O que você quer aprender? :\n1 - O que é Javascript\n2 - Conceitos fundamentais\n3 - Funções";
                return new
                {
                    fulfillmentMessages = new[] { new { text = new { text = new[] { subMenu } } } },
                    outputContexts = new[] {
                        new { name = $"{session}/contexts/esperando_menu_principal", lifespanCount = 0, parameters = new {} },
                        new { name = $"{session}/contexts/aguardando_escolha_curso", lifespanCount = 5, parameters = new {} }
                    }
                };
            }

            if (intentName == "exercicio" || textoUsuario == "2")
                return IniciarAtividade(session, ListaQuiz, "fazendo_quiz", "🚀 QUIZ JS");

            if (intentName == "questionario" || textoUsuario == "3")
                return IniciarAtividade(session, ListaQuestionario, "fazendo_questionario", "📝 QUESTIONÁRIO");

            // Fallback (Se digitar algo fora das opções do menu)
            return RespostaComMenu(session, $"Não entendi. {TextoMenu}");
        }

        // ================== FLUXO ATIVIDADES ==================

        private object IniciarAtividade(string session, List<PerguntaQuiz> lista, string contexto, string titulo)
        {
            var p = lista.First();
            return MontarResposta(session, $"{titulo} (1/{lista.Count})\n\n{p.Texto}\n{p.Opcoes}", 1, 0, contexto);
        }

        private object ProcessarAtividade(JsonElement payload, string session, string resposta, List<PerguntaQuiz> lista, string contexto, bool comFeed)
        {
            var queryResult = payload.GetProperty("queryResult");
            int idAtual = 1; int pontos = 0;

            if (queryResult.TryGetProperty("outputContexts", out JsonElement contexts))
            {
                foreach (var c in contexts.EnumerateArray())
                {
                    var name = c.TryGetProperty("name", out JsonElement n) ? n.GetString() : "";
                    if (name.Contains(contexto))
                    {
                        if (c.TryGetProperty("parameters", out JsonElement paras))
                        {
                            idAtual = ExtrairInt(paras, "id_pergunta", 1);
                            pontos = ExtrairInt(paras, "pontuacao_atual", 0);
                        }
                    }
                }
            }

            var pAtual = lista.FirstOrDefault(p => p.Id == idAtual);
            if (pAtual == null) return ResetarContextos(session);

            bool acertou = (resposta == pAtual.Correta);
            if (acertou) pontos += comFeed ? 10 : 1;

            var proxima = lista.FirstOrDefault(p => p.Id == idAtual + 1);

            if (proxima != null)
            {
                string msg = "";
                if (comFeed)
                    msg = acertou ? $"✅ Correto! {pAtual.Feedback}\n\n" : $"❌ Errado! Era {pAtual.Correta}. {pAtual.Feedback}\n\n";

                msg += $"Próxima ({proxima.Id}/{lista.Count}):\n{proxima.Texto}\n{proxima.Opcoes}";
                return MontarResposta(session, msg, proxima.Id, pontos, contexto);
            }

            string fim = comFeed
                ? $"🏆 QUIZ CONCLUÍDO! Nota Final: {pontos}/100\n\n{TextoMenu}"
                : $"📊 QUESTIONÁRIO FINALIZADO!\n✅ Acertos: {pontos}\n❌ Erros: {lista.Count - pontos}\n\n{TextoMenu}";

            return new
            {
                fulfillmentMessages = new[] { new { text = new { text = new[] { fim } } } },
                outputContexts = new[] { 
                    new { name = $"{session}/contexts/{contexto}", lifespanCount = 0, parameters = new {} },
                    new { name = $"{session}/contexts/esperando_menu_principal", lifespanCount = 5, parameters = new {} }
                }
            };
        }

        // ================== HELPERS E MODERAÇÃO ==================
        
        private bool EstaEmContexto(JsonElement queryResult, string nomeContexto)
        {
            if (queryResult.TryGetProperty("outputContexts", out JsonElement contexts))
            {
                foreach (var c in contexts.EnumerateArray())
                {
                    var name = c.TryGetProperty("name", out JsonElement n) ? n.GetString() : "";
                    var lifespan = c.TryGetProperty("lifespanCount", out var lc) ? lc.GetInt32() : 0;
                    if (name.Contains(nomeContexto) && lifespan > 0) return true;
                }
            }
            return false;
        }

        private object ResetarContextos(string session)
        {
            return new
            {
                fulfillmentMessages = new[] { new { text = new { text = new[] { $"🔄 Atividade encerrada. Digite 'oi' paara me chamar novamente" } } } },
                outputContexts = new[] {
                    new { name = $"{session}/contexts/fazendo_quiz", lifespanCount = 0, parameters = new {} },
                    new { name = $"{session}/contexts/fazendo_questionario", lifespanCount = 0, parameters = new {} },
                    new { name = $"{session}/contexts/aguardando_escolha_curso", lifespanCount = 0, parameters = new {} },
                    new { name = $"{session}/contexts/esperando_menu_principal", lifespanCount = 5, parameters = new {} }
                }
            };
        }

        private object RespostaComMenu(string session, string texto)
        {
            return new
            {
                fulfillmentMessages = new[] { new { text = new { text = new[] { texto } } } },
                outputContexts = new[] {
                    new { name = $"{session}/contexts/esperando_menu_principal", lifespanCount = 5, parameters = new {} }
                }
            };
        }

        private object RespostaSimples(string texto)
            => new { fulfillmentMessages = new[] { new { text = new { text = new[] { texto } } } } };

        private object AplicarPunicao(StatusModeracao status, string session)
        {
            status.ContadorAvisos++;
            if (status.ContadorAvisos >= 2) status.BloqueadoAte = DateTime.Now.AddMinutes(5);
            if (status.ContadorAvisos >= 3) status.BloqueioDefinitivo = true;
            return RespostaSimples("⚠️ Linguagem inadequada detectada.");
        }

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