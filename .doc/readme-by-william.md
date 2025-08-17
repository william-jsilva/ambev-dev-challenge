# Considera��es finais

## Diverg�ncias com o que foi solicitado

1. O BaseEntity.Id e CreateUserResult.Id que j� est�o no template s�o do tipo Guid, por�m a documenta��o, como em [POST /carts](carts-api.md#post-carts) foi definido como inteiro. Mantemos o tipo Guid para os identificadores do projeto.
1. A classe base ApiResponse possui as propriedades Errors e Success, que n�o foram solicitadas na documenta��o. Elas foram mantidas para compatibilidade com o template.


## Propostas de evolu��es futuras

1. Rota de categorias de produtos (sitar o modelo de maturidade de richarson)
1. Retirar os atributos sucesso e erros da classe ApiResponse.
1. A valida��o dos dados est� triplicada (Request, Command e Entidade). Centralizar as valida��es quando poss�vel.
1. Incluir autoriza��o nas APIs de usu�rio, carrinho, venda e produto (Autorize)
1. Incluir tratamento para excess�es (KeyNotFound = 404, Neg�cio 422, UnauthorizedAccessException com 401 e demais casos com 500)
1. 1. Ajustar o recurso de usu�rio (fiz apenas ajusters pontuais para funcionamento das opera��es atuais):
	- Ajustar o response do POST
	- Avaliar a altera��o da exclus�o f�sica para l�gica
	- Incluir PUT e GET (filtros)
	- Verificar a role/claim para exclus�o e altera��o (usu�rios "comuns" s� podem excluir a si mesmo)
	- Incluir ativa��o de usu�rio por e-mail (ou outras formas adicionais, e-mail, SMS, MFA)
	- Renomear username para name (O acesso/autentica��o est� sendo realizado atrav�s do e-mail)

## Observa��es

1. TBD
1. Para Facilitar os testes foi adicionado o authorize apenas na rota de dele��o de usuario

[Back to README](../README.md)

