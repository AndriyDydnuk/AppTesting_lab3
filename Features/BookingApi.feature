Feature: Booking API CRUD Operations
  ��� ��������� ������ Restful Booker API,
  �� QA �������,
  � ���� �������� �������� CRUD ��� ������������.

  Scenario: GET �� ����������
    Given I have the Restful Booker API endpoint "http://restful-booker.herokuapp.com"
    When I send a GET request to "booking"
    Then the response status code should be 200

  Scenario: POST ��������� ������ ����������
    Given I have the Restful Booker API endpoint "http://restful-booker.herokuapp.com"
    When I send a POST request to "booking" with body:
      """
      {
         "firstname": "John",
         "lastname": "Doe",
         "totalprice": 150,
         "depositpaid": true,
         "bookingdates": {
            "checkin": "2025-06-01",
            "checkout": "2025-06-10"
         },
         "additionalneeds": "Breakfast"
      }
      """
    Then the response status code should be 418

  Scenario: PUT ��������� ����������
    Given I have the Restful Booker API endpoint "http://restful-booker.herokuapp.com"
    And I have an existing booking with id created earlier POST operation
    When I send a PUT request to "booking/{id}" with body:
      """
      {
         "firstname": "John",
         "lastname": "Smith",
         "totalprice": 200,
         "depositpaid": false,
         "bookingdates": {
            "checkin": "2025-06-05",
            "checkout": "2025-06-15"
         },
         "additionalneeds": "Lunch"
      }
      """
    Then the response status code should be 405

  Scenario: DELETE ��������� ����������
    Given I have the Restful Booker API endpoint "http://restful-booker.herokuapp.com"
    And I have an existing booking with id created earlier POST operation
    When I send a DELETE request to "booking/{id}"
    Then the response status code should be 405
