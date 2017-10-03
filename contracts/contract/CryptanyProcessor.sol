pragma solidity ^0.4.11;

  /**
   * @dev Prototype for internal testing
   */
contract CryptanyProcessor {
    
  /**
   * @dev Structure for all incoming/outcoming payments
   */
    struct PaymentStructure {
    
        address fromAddress;

        address toAddress;

        uint paymentSize;
        
        string comment;
    }


  /**
   * @dev Structure for transactions validation
   */    
    struct ValidationStructure {
        
        address validator;
        
        bool result;
        
        string comment;
    }
    
    //address of contract owner
    address  public owner;
    
    //all payments agregated by this contract
    uint public balance;
    
    //list of allowed verifiers - might be refactored
    mapping (address => address) verifiers;
    
    //mapping betwen transaction hash and all validators
    mapping (bytes32 => ValidationStructure) pendingValidations;
    
    //mapping betwen transaction hash and payment structure
    mapping (bytes32 => PaymentStructure) pendingPayments;
    
    /**
     * @dev Throws if called by any account other than the owner.
     */
    modifier onlyOwner() {
        require(msg.sender == owner);
        _;
    }
    
   /**
     * @dev Throws if called by any account other than the validators.
     */
    modifier onlyValidators() {
        require(verifiers[msg.sender] == msg.sender);
        _;
    }
    
   /**
    * @dev event occurs in each transaction
    */
    event sendMoneyEvent(
        address indexed toPerson, 
        address indexed fromPerson,
        uint payment,
        string comment,
        bytes32 transactionHash
        );
        
   /**
    * @dev event occurs in each positive validation
    */    
    event transactionApproved(
        bytes32 indexed transactionHash,
        address validator,
        string comment
        );
        
   /**
    * @dev event occurs in each rejection
    */    
    event transactionRejected(
        bytes32 indexed transactionHash,
        address validator,
        string comment
        );        
        
        
   /**
    * @dev event occurs when new validatior is added/removed
    * 
    */    
    event changeListOfValidators(
        string operationType,
        address validator
        );
        
   /**
    * @dev The Ownable constructor sets the original `owner` of the contract to the sender
    * account.
    */
    function Ownable() {
      owner = msg.sender;
    }    

   /**
    * @dev Main function to create transaction
    */
    function sendMoneyFor(address toPerson, string comment) payable returns (bytes32) {
        
        //Seller balance should bigger, than any payment
        require (toPerson.balance >= msg.value);
        
        //var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment);
        var pendingPay = PaymentStructure(msg.sender, toPerson, msg.value, comment);
        
        //hash from all concatinated data
        var transactionHash = sha3(msg.sender, toPerson, msg.value, block.timestamp);

        pendingPayments[transactionHash]=pendingPay;
        balance += msg.value;
        
        //sendMoneyEvent(toPerson, msg.sender, msg.value, comment, transactionHash);
        sendMoneyEvent(toPerson, msg.sender, msg.value, comment, transactionHash);
        
        //new transaction shoul be added into the pending chain
        var pendingValidation = ValidationStructure(verifiers[1], false, "Validation required");
        pendingValidations[transactionHash] = pendingValidation;
        
        return transactionHash;
    }
    
   /**
    * @dev Main function to process transaction
    */
    function receiveMoney(bytes32 transactionHash) returns (bool) {
        PaymentStructure storage p = pendingPayments[transactionHash];
        
        //Only money receiver may call this function
        require (msg.sender == p.toAddress);
        
        //only approved transaction may commited
        require (pendingValidations[transactionHash].validator == 0);
        
        //transaction from contract to address
        p.toAddress.transfer(p.paymentSize);
        
        //removing payment element
        delete pendingPayments[transactionHash];
        
        return true;
        
    }
    
    function addAllowedValidator(address validatorAddress) onlyOwner {
        verifiers[validatorAddress] = validatorAddress;
        changeListOfValidators("New validator has been added", validatorAddress);
    }
    
    function removeAllowedValidator(address validatorAddress) onlyOwner {
        //only existing validator can be removed
        require (verifiers[validatorAddress]!=0);
        changeListOfValidators("Validator has been removed", validatorAddress);
    }
    
   /**
    * @dev function to approve transaction. Should work with many validators.
    */
    function provideValidation(bytes32 transaction) onlyValidators {
        
        var validation = pendingValidations[transaction];

        if (validation.validator == msg.sender && validation.result == true){
            delete pendingValidations[transaction]; 
            transactionApproved(transaction, validation.validator, validation.comment);
        }  
        else
            transactionRejected(transaction, validation.validator, validation.comment);
        
    }

}