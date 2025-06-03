Feature: Joke API Testing
  Щоб перевірити роботу API випадкових жартів,
  як QA інженер,
  я хочу отримати випадковий жарт.

  Scenario: Отримання випадкового жарту
    Given I have the Joke API endpoint "https://api.chucknorris.io"
    When I send a GET request to "jokes/random"
    Then the response status code should be 200
