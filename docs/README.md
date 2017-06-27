# mpos-net-sdk

SDK para .Net para comunicação com pinpads e mPOS.

## Setup Windows

1. Utilize o `nuget` para receber o pacote.

## Documentação

### Mpos

A classe `Mpos` é responsável pelo gerenciamento de todos os fluxos do SDK para .Net. Seus métodos estão documentados a seguir:

#### Métodos

##### `Mpos(Stream stream, string encryptionKey, string storagePath)`

Inicializa uma instância da classe `Mpos`. Os parâmetros necessários são:

* `stream`: objeto da classe `Stream`, referente ao pinpad com o qual o usuário está pareado via Bluetooth ou USB. Para obter este objeto, é recomendado o uso da classe `SerialPort` e sua propriedade `BaseStream`, documentadas na MSDN.
* `encryptionKey`: a encryption key pagar.me utilizada para geração do card hash junto à API. Pode ser obtida na Dashboard.
* `storagePath`: caminho onde serão salvas as tabelas da mpos.

##### `Initialize()`

Abre uma sessão com o pinpad. Não recebe parâmetros.

Se usado com `await`, retorna um booleano com o status da operação. Tem associado o evento `Initialized`.

##### `Close()`

Fecha uma sessão com o pinpad. Não recebe parâmetros.

Não retorna. Tem associado o evento `Closed`.

##### `SynchronizeTables(bool forceUpdate)`

Baixa da API pagar.me as tabelas EMV mais recentes em disponibilidade, e, se necessário, as transfere ao pinpad. Guarda as tabelas no caminho indiciado no parâmetro `storagePath`, passado no construtor. Recebe os seguintes parâmetros:

* `forceUpdate`: Determina se, independentemente da necessidade de uma atualização das tabelas no pinpad, elas devem ser transferidas.

Se usado com `await`, retorna um booleano com o status da operação. Tem associado o evento `TableUpdated`.

##### `Display(string text)`

Requisita ao pinpad que uma mensagem seja mostrada em seu display. Recebe os seguintes parâmetros:

* `text`: Texto a ser mostrado pelo pinpad.

Não retorna.

##### `ProcessPayment(int amount, IEnumerable<EmvApplication> applications, PaymentMethod magstripePaymentMethod)`

Requisita ao pinpad que seja processado um pagamento. Recebe os seguintes parâmetros:

* `amount`: Número, em centavos, que indica a quantia cobrada.
* `applications`: Indica as aplicações suportadas pelo pagamento. Se não passado, usará as registradas.
* `magstripePaymentMethod`: Caso esteja passando tarja, indica o tipo de cartão: Crédito ou Débito. Se não passado, usará como Crédito.

Se usado com `await`, retorna um objeto `PaymentResult`. Tem associado o evento `PaymentProcessed`.

##### `FinishPayment(bool success, int responseCode, string emvData)`

Requisita ao pinpad que seja finalizado um pagamento. Recebe os seguintes parâmetros:

* `success`: Indica se a transação foi bem sucedida na adquirente e deve ser paga.
* `responseCode`: Código de resposta da adquirente.
* `emvData`: Dados encriptados sobre a operação EMV enviados pelo adquirente.

Não retorna. Tem associado o evento `FinishedTransaction`.

##### `Cancel()`

Cancela a operção no pinpad. Não recebe parâmetros. Não retorna.

#### Eventos

##### `Initialized`

Evento chamado quando uma sessão com o pinpad termina de ser inicializada. Recebe os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `args`: Objeto `EventArgs` vazio.

##### `Closed`

Evento chamado quando uma sessão com o pinpad é fechada. Recebe os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `args`: Objeto `EventArgs` vazio.

##### `TableUpdated`

Evento chamado quando um pedido de atualização de tabelas é terminado. Recebe os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `loaded`: Booleano que indica se houve necessidade de transferir as tabelas baixadas da API Pagar.me ao pinpad.

##### `PaymentProcessed`

Evento chamado quando um pagamento termina de ser processado junto ao pinpad. Recebe os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `result`: Objeto `PaymentResult` que contém dados do pagamento efetuado.

##### `FinishedTransaction`

Evento chamado quando um pagamento é finalizado junto ao pinpad. Recebe os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `args`: Objeto `EventArgs` vazio.

##### `NotificationReceived`

Evento chamado quando o pinpad envia uma notificação que deve ser mostrada ao usuário final.

* `sender`: Objeto da classe `Mpos`.
* `notification`: String que contém uma notificação a ser mostrada para o usuário com uma mudança de status do pinpad.

##### `OperationCompleted`

Evento chamado quando uma operação junto ao pinpad é concluída.

* `sender`: Objeto da classe `Mpos`.
* `args`: Objeto `EventArgs` vazio.

##### `Errored`

Evento chamado quando há um erro na comunicação com o pinpad.

* `sender`: Objeto da classe `Mpos`.
* `args`: Número correspondente a um `abecs_stat_t` que indica um erro.

### PaymentResult

Classe que possui como propriedades dados de retorno de um pagamento.

##### `CardHash`

String que representa a card hash do cartão a ser enviada à API Pagar.me.

##### `Status`

Valor da enum `PaymentStatus` que representa o status do pagamento processado.

##### `PaymentMethod`

Valor da enum `PaymentMethod` que representa o método a ser utilizado para o processamento da transação junto ao adquirente.

##### `CardHolderName`

String que representa o nome do portador do cartão.

##### `IsOnlinePin`

Sinaliza se a operação foi feita com checagem de senha online ou no cartão.

### PaymentStatus

`Enum` que mostra o status de um pagamento processado. Possui os seguintes valores:

* `Accepted`: O pagamento foi aceito.
* `Rejected`: O pagamento foi rejeitado.
* `Errored`: Houve um erro no processamento do pagamento.

### PaymentMethod

`Enum` que mostra o método a ser utilizado para o processamento da transação junto ao adquirente. Possui os seguintes valores:

* `Credit` (1): Crédito.
* `Debit` (2): Débito.

## Exemplo

```cs
using System.UI.Ports;
using PagarMe;
using PagarMe.Mpos;

String device = "COM4"; // porta serial do pinpad
String encryptionKey = "ek_test_XXX"; // encryption key Pagar.me

SerialPort port = new SerialPort(device, 240000, Parity.None, 8, StopBits.One);
Mpos mpos = new Mpos(port.BaseStream, encryptionKey);

// Adiciona um listener para o evento NotificationReceived
mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);

// Aguarda inicialização de uma sessão
await mpos.Initialize();

// Inicia um pagamento e aguarda seu término
PaymentResult r = await mpos.ProcessPayment(100);
Console.WriteLine(r.CardHash); // Imprime a card hash a ser enviada à API pagar.me
};
```

Para exemplo mais detalhado: [projeto usado para testes](../../PagarMe.Mpos.Example)
