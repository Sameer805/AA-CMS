//Alpha = (name)
//Aplhanumeric = (password,username)
//Datetime

//Properties 
/*  id = Actual attribute Id of input field             <REQUIRED>
 *  type = data type of inputfield                      <REQUIRED>
 *          aplha = only alphabets
 *          aplhanumeric = aplhabets with numbers
 *          datatime = datetime
 *          date = date 
 *          checkbox = checkbox
 *          combobox = combobox
 *  required = is field is required to be filled
 *  match = match the other field value
 *  format = pattern
 *          
 *          
 */


//alert(ValidateInputFields(arr));

function ValidateInputFields(arr) {
    var finalStatus = false;
    if (arr.length == 1) {
        return Check(arr,0);
    }
    else {
        $(arr).each(function (index, _element) {
            Check(_element,index);     
        });
    }
}
function Check(element, index) {
    var StatusBools;

    for (var i = 0; i < (index + 1); i++) {
        var ele = element[index];
        var value = $(ele.id).val();
        //Checking required 
        if (ele.hasOwnProperty("required")) {
            //Has required
            if (ele.required == true) {
                if (value == "" || value == null || value == undefined) {
                    //It is empty
                    return "empty";
                }
            }
        }
        if (ele.type == "alpha") {
            if (/[^a-zA-Z]/.test(value)) {
                return "";
                //Eror (Value have numeric)
            }
        }
        if (ele.type == "aplhanumeric") {
            if ("^[a-zA-Z0-9_]*$".test(value)) {
                //Error (Value isn't alphanumeric)
            }
        }
    }
}
