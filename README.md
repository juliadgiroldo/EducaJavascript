Um bom **README** é essencial para que outros desenvolvedores (ou os seus avaliadores do trabalho) consigam rodar o seu projeto sem dificuldades.

Aqui está uma estrutura completa e profissional para o seu repositório no GitHub:

---

# 🚀 Educa JavaScript API

O **Educa JavaScript** é um agente conversacional pedagógico desenvolvido para auxiliar no ensino da linguagem JavaScript. A solução integra um sistema de moderação de conteúdo, módulos de explicação teórica com exemplos práticos e avaliações via Quiz e Questionário.

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# 10.0
* **Framework:** ASP.NET Core 10
* **NLU:** Google Dialogflow ES
* **Plataforma:** Telegram (via Dialogflow)

---

## 📥 Como baixar e rodar o projeto

### 1. Pré-requisitos

Certifique-se de ter instalado em sua máquina:

* [.NET SDK 10.0]
* [Visual Studio Code](https://www.google.com/search?q=https://code.visualstudio.com/)

### 2. Clonagem e Instalação

No terminal do seu computador, execute:

```bash
# Clone o repositório
git clone https://github.com/juliadgiroldo/EducaJavascript.git

# Acesse a pasta do projeto
cd EducaJavascript

# Restaure as dependências
dotnet restore

# Execute a aplicação
dotnet run

```

A API estará rodando localmente, geralmente na porta `5000` ou `5001`.

---

## 🌐 Configurando o Túnel HTTPS (VS Code Ports)

Como o Dialogflow exige uma URL pública e segura (HTTPS) para se comunicar com a sua API local, utilizaremos o recurso nativo do VS Code:

1. Com o projeto rodando, procure a aba **Ports** (Portas) no painel inferior do VS Code (ao lado do Terminal).
2. Clique em **Forward a Port** (Encaminhar uma Porta).
3. Digite o número da porta onde sua API está rodando (ex: `5000`).
4. **IMPORTANTE:** Na coluna **Visibility** (Visibilidade), clique com o botão direito e altere de *Private* para **Public**. Se deixar como *Private*, o Dialogflow não conseguirá acessar.
5. Copie a URL gerada na coluna **Forwarded Address**.

---

## 🤖 Configuração no Dialogflow

Para conectar o chatbot à sua API:

1. Acesse o console do [Dialogflow ES](https://www.google.com/search?q=https://dialogflow.cloud.google.com/).
2. No menu lateral esquerdo, clique em **Fulfillment**.
3. Habilite a opção **Webhook** caso não esteja ativado.
4. No campo **URL**, cole o endereço que você copiou do VS Code e adicione o caminho do seu endpoint no final.
* Exemplo: `https://sua-url-do-vscode.app/api/dialogflow/webhook`


5. Role até o final da página e clique em **Save**.
---

## 🧠 Estrutura de Lógica

O sistema utiliza uma arquitetura separada em camadas:

* **Controllers:** Responsáveis pelo recebimento e formatação das requisições do Dialogflow.
* **UseCases:** Onde reside toda a lógica de negócio, incluindo a máquina de estados de moderação e o motor de correção de exercícios.
* **Models:** Estruturas de dados para o Quiz, Questionário e Status de Moderador.

---

## ⚖️ Sistema de Moderação

A API monitora as interações e aplica punições progressivas:

1. **Aviso:** Feedback educativo.
2. **Suspensão:** Bloqueio temporário de 5 minutos (via código).
3. **Banimento:** Bloqueio definitivo da sessão após 3 infrações.

---

**Desenvolvido por:** Julia Giroldo 👩‍💻
