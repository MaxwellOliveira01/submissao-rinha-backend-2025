# Rinha de Backend 2025

## Desafio

O desafio dessa edição da rinha consistia em construir uma api que recebesse requests de pagamentos e redirecionasse para dois processadores de pagamentos, o default e o fallback. O objetivo é processar o máximo de pagamentos, mas tem alguns detalhes que tornam isso um pouco mais delicado:
- Durante o teste, cada processador de pagamento alterna entre três estados: saudavel, fora do ar e tempo de resposta alto
- Cada processador cobra uma taxa em cima do valor do pagamento, sendo que a taxa do fallback é o dobro da taxa do default
- Assim que o teste encerra, todos os pagamentos não processados não são mais contabilizados.

O desafio também possui regras como: sua aplicação deve ter no mínimo duas instâncias, utilizad no maximo 350mb de memória e 1.5 de CPU.

Além disso tudo, a api precisa implementar uma rota que retorne estatísticas dos pagamentos. Isso é, dizer, para cada processador, quantos pagamentos foram realizados lá e o valor total acumulado.

As estatísticas trazem um nivel de complexidade maior, pois é dificil manter a consistência dos dados. Quando sua aplicação responde errado, uma "inconsistência" é encontrada e uma multa será aplicada ao final.

Por fim, quanto mais rápido for o tempo de resposta da aplicação, melhor. Se o p99 for abaixo de 10ms, haverá um bonus sobre o resultado final.

Mais informações podem ser encontradas no [repositório oficial](https://github.com/zanfranceschi/rinha-de-backend-2025).

## Solução

O projeto foi feito usando C# / .NET, Redis e Nginx, descreveremos melhor a ideia a seguir.

- Quando uma requisição chega, o pagamento é colocado em uma fila do Redis (que é compartilhada entre as 2 instancias) e um sucesso é retornado. Note que o pagamento ainda não foi realmente processado, apenas enfileirado. A ideia neste ponto é responder rapidamente com um 200.
- Após ser enfileirado, existe um serviço e alguns workers com objetivo de tirar da fila e processar. Eles tentam processar e, caso dê errado, enfileiram novamente.
- Quando um pagamento for ser processado, a preferência é sempre do processador default. Se ele estiver fora do ar, o dado é reenfileirado para tentar novamente**.

** Essa foi a estratégia que, durante os testes, mais trouxe benefício. Outras estratégias como "usar o default e, se nao der, usa o fallback" trouxeram resultado um pouco inferior.

O Nginx foi usado como Load balancer entre as instâncias para dividir igualmente os requests entre as instâncias.

O Redis foi utilizado de duas formas:
  - Como fila compartilhada para manter os pagamentos ainda não processados
  - Como Banco de dados para conseguir responder as estatísticas rapidamente.

## Resultado

No teste final, essa solução garantiu um bom resultado, me deixando em 21o dentre mais de 300 participantes. O ranking final pode ser encontrado [aqui](https://github.com/zanfranceschi/rinha-de-backend-2025/blob/main/RESULTADOS_FINAIS.md)
