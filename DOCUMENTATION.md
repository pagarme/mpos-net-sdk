# mpos-windows-sdk

SDK para .Net para comunicação com pinpads e mPOS.

## Setup

1. Utilize o `nuget` para receber o pacote.

## Documentação

### Mpos

A classe `Mpos` é responsável pelo gerenciamento de todos os fluxos do SDK para .Net. Seus métodos estão documentados a seguir:

#### Métodos

##### `Mpos(Stream stream, string encryptionKey)`

Inicializa uma instância da classe `Mpos`. Os parâmetros necessários são:

* `stream`: objeto da classe `Stream`, referente ao pinpad com o qual o usuário está pareado via Bluetooth ou USB. Para obter este objeto, é recomendado o uso da classe `SerialPort` e sua propriedade `BaseStream`, documentadas na MSDN.
* `encryptionKey`: a encryption key pagar.me utilizada para geração do card hash junto à API. Pode ser obtida na Dashboard.

##### `Initialize()`

Abre uma sessão com o pinpad. Não toma parâmetros.

Se usado com `await`, retorna um booleano com o status da operação. Tem associado o evento `Initialized`.

##### `Close()`

Fecha uma sessão com o pinpad. Não toma parâmetros.

Não retorna.

##### `SynchronizeTables(bool forceUpdate)`

Baixa da API pagar.me as tabelas EMV mais recentes em disponibilidade, e, se necessário, as transfere ao pinpad. Toma os seguintes parâmetros:

* `forceUpdate`: Determina se, independentemente da necessidade de uma atualização das tabelas no pinpad, elas devem ser transferidas.

Se usado com `await`, retorna um booleano com o status da operação. Tem associado o evento `TableUpdated`.

##### `Display(string text)`

Requisita ao pinpad que uma mensagem seja mostrada em seu display. Toma os seguintes parâmetros:

* `text`: Texto a ser mostrado pelo pinpad.

Não retorna.

##### `ProcessPayment(int amount, PaymentFlags flags)`

Requisita ao pinpad que seja processado um pagamento. Toma os seguintes parâmetros:

* `amount`: Número, em centavos, que indica a quantia cobrada.
* `flags`: Indica as aplicações suportadas pelo pagamento. Opcional (padrão é `PaymentFlags.Default`).

Se usado com `await`, retorna um objeto `PaymentResult`. Tem associado o evento `PaymentProcessed`.

#### Eventos

##### `Initialized`

Evento chamado quando uma sessão com o pinpad termina de ser inicializada. Toma os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `args`: Objeto `EventArgs` vazio.

##### `PaymentProcessed`

Evento chamado quando um pagamento termina de ser processado junto ao pinpad. Toma os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `result`: Objeto `PaymentResult` que contém dados do pagamento efetuado.

##### `TableUpdated`

Evento chamado quando um pedido de atualização de tabelas é terminado. Toma os seguintes parâmetros:

* `sender`: Objeto da classe `Mpos`.
* `loaded`: Booleano que indica se houve necessidade de transferir as tabelas baixadas da API Pagar.me ao pinpad.

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

### PaymentFlags

`Enum` que traduz para .Net a estrutura `mpos_payment_flags_t` da Define os tipos de aplicação que podem ser requisitados para limitar o processamento de pagamento. Podem ser complementados usando o operador bitwise OR (`|`).

* `None`: Nenhuma aplicação.
* `CreditCard`: Aplicação de cartão de crédito.
* `DebitCard`: Aplicação de cartão de débito.
* `AllApplications`: Aplicações de Crédito e Débito.
* `Visa`: Aplicação de cartão Visa.
* `MasterCard`: Aplicação de cartão Master.
* `AllBrands`: Aplicações de cartão Visa e Master.
* `Default`: Todas as aplicações disponíveis.

A constante `CreditCard` ou `Visa` seriam, por si só, impróprias para a utilização de qualquer aplicação, uma vez que uma especifica uma modalidade de pagamento e outra uma bandeira, sem especificar o outro. Em compensação, a combinação `CreditCard | Visa`, para requisitar somente Visa Crédito, é válida.

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

