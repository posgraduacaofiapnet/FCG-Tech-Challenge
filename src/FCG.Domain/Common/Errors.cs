namespace FCG.Domain.Common;

public static class Errors
{
    public static class Auth
    {
        public static Error InvalidCredentials => Error.Unauthorized("Auth.InvalidCredentials", "Credenciais invalidas.");
        public static Error RegisterFailed => Error.Validation("Auth.RegisterFailed", "Nao foi possivel concluir o cadastro com os dados informados.");
        public static Error WeakPassword => Error.Validation("Auth.WeakPassword", "Senha deve ter no minimo 8 caracteres, letras, numeros e caracteres especiais.");
    }

    public static class Games
    {
        public static Error NotFound => Error.NotFound("Games.NotFound", "Jogo nao encontrado.");
    }

    public static class Orders
    {
        public static Error NotFound => Error.NotFound("Orders.NotFound", "Pedido nao encontrado.");
        public static Error AccessDenied => Error.Forbidden("Orders.AccessDenied", "Usuario nao possui permissao para acessar este pedido.");
        public static Error EmptyGames => Error.InvalidRequest("Orders.EmptyGames", "Informe ao menos um jogo para criar o pedido.");
        public static Error InvalidStatus(string message) => Error.Validation("Orders.InvalidStatus", message);
        public static Error NotPending(Guid orderId) => InvalidStatus($"Pedido com ID {orderId} nao esta em status Pendente.");
        public static Error CannotAddItemsToNonPendingOrder => InvalidStatus("Nao e possivel adicionar itens a um pedido que nao esta pendente.");
        public static Error OnlyPendingOrdersCanBePaid => InvalidStatus("Apenas pedidos pendentes podem ser marcados como pagos.");
        public static Error OnlyPendingOrdersCanBeCanceled => InvalidStatus("Apenas pedidos pendentes podem ser cancelados.");
    }

    public static class Pagination
    {
        public static Error InvalidPage => Error.InvalidRequest("Pagination.InvalidPage", "Pagina deve ser maior ou igual a 1.");
    }

    public static class Promotions
    {
        public static Error NotFound => Error.NotFound("Promotions.NotFound", "Promocao nao encontrada.");
    }

    public static class UnitOfWork
    {
        public static Error CommitFailed => Error.Failure("UnitOfWork.CommitFailed", "Nao foi possivel concluir a operacao no banco de dados.");
    }

    public static class Users
    {
        public static Error NotFound => Error.NotFound("Users.NotFound", "Usuario nao encontrado.");
        public static Error NotFoundByEmail => Error.NotFound("Users.NotFoundByEmail", "Usuario nao encontrado.");
        public static Error InvalidRole => Error.InvalidRequest("Users.InvalidRole", "Role informada e invalida.");
        public static Error DefaultAdminCannotBeDeleted => Error.Validation("Users.DefaultAdminCannotBeDeleted", "O administrador default da aplicacao nao pode ser excluido.");
        public static Error EmailAlreadyRegistered => Error.Validation("Users.EmailAlreadyRegistered", "Ja existe um usuario cadastrado com o e-mail informado.");
    }
}
