# Considerações finais

## Divergências com o que foi solicitado

1. O BaseEntity.Id e CreateUserResult.Id que já estão no template são do tipo Guid, porém a documentação, como em [POST /carts](carts-api.md#post-carts) foi definido como inteiro. Mantemos o tipo Guid para os identificadores do projeto.
1. A classe base ApiResponse possui as propriedades Errors e Success, que não foram solicitadas na documentação. Elas foram mantidas para compatibilidade com o template.


## Propostas de evoluções futuras

1. Rota de categorias de produtos (sitar o modelo de maturidade de richarson)
1. Retirar os atributos sucesso e erros da classe ApiResponse.
1. A validação dos dados está triplicada (Request, Command e Entidade). Centralizar as validações quando possível.
1. Incluir autorização nas APIs de usuário, carrinho, venda e produto (Autorize)
1. Incluir tratamento para excessões (KeyNotFound = 404, Negócio 422, UnauthorizedAccessException com 401 e demais casos com 500)
1. 1. Ajustar o recurso de usuário (fiz apenas ajusters pontuais para funcionamento das operações atuais):
	- Ajustar o response do POST
	- Avaliar a alteração da exclusão física para lógica
	- Incluir PUT e GET (filtros)
	- Verificar a role/claim para exclusão e alteração (usuários "comuns" só podem excluir a si mesmo)
	- Incluir ativação de usuário por e-mail (ou outras formas adicionais, e-mail, SMS, MFA)
	- Renomear username para name (O acesso/autenticação está sendo realizado através do e-mail)

## Observações

1. TBD
1. Para Facilitar os testes foi adicionado o authorize apenas na rota de deleção de usuario

[Back to README](../README.md)

