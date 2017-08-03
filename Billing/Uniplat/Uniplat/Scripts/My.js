function findOperator(element, change) {
    /*jQuery(change).load('/logik.php?numb=' + element.value.replace(" ", ""));*/
    $.post(
      "../Projects/FindOperator",
      {
          param1: element.value.replace(" ", "")
      },
      function (data) { $(change).text(data); }
    );
  }

  function countMoney(element, change) {
      /*jQuery(change).load('/logik.php?numb=' + element.value.replace(" ", ""));*/
      $.post(
      "../Projects/CountMoney",
      {
          param1: element.value
      },
      function (data) { $(change).text(data); }
    );
  }


  function pay(number, money) {
      $.post(
      "../Projects/Pay",
      {
          param1: $(number)[0].value,
          param2: $(money)[0].value
      },
      function (data) {
          if (data == "ok") {
              alert("Платеж прошел успешно");
          }
          else {
              alert("При платеже произошла ошибка");
          }
      }
    );
  }


  function paymentState() {
      $.get(
      "/Projects/State",
      {},
      function (data) {
          modalWindowText.innerHTML = data;
          $('#modalWindow').arcticmodal();
      }
    );
  }
  

jQuery(function ($) {

    $("#phone2").mask("(999) 999-99-99");
    $("#phone1").mask("(999) 999-99-99");

});