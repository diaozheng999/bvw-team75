
$(document).ready(() => {
    let cart = {};
    let budget = 1000;
    
    let renderCart = () => {
        if($.isEmptyObject(cart)) {
            console.log("Cart is empty/");
            $('#cart-body').html("<p>Cart is empty.</p>");
        } else {
            $('#cart-tbody').empty()
            $.each(cart, (i, v) => {
                let tr = $('<tr>')
                tr.append(`
                    <td>${itemDictionary[i].title}</th>
                    <td>${v}</td>
                    <th scope="row">$${itemDictionary[i].price * v}</td>
                `)

                let actioncol = $('<td>')
                
                let add = $('<a href="#"><i class="fa fa-plus" aria-hidden="true"></i></a>')
                add.on('click', () => {
                    if(budget >= itemDictionary[i].price) {
                        budget -= itemDictionary[i].price;
                        if (i in cart) {
                            cart[i] ++;
                        } else {
                            cart[i] = 1;
                        }
                        $('#budget').text(budget);
                        renderCart();
                    } else {
                        let _p = $(`<div class="alert alert-danger alert-dismissible fade show" role="alert">
                        Not enough money.
                        <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                        </div>`);

                        $('#modal-alert-wrapper').append(_p);
                        $('#budget').text(budget);
                        setTimeout(() => {
                            _p.alert('close')
                        }, 3000);
                    }
                })

                let sub = $('<a href="#"><i class="fa fa-minus" aria-hidden="true"></i></a>')

                sub.on('click', () => {
                    budget += +itemDictionary[i].price;
                    cart[i]--;
                    if(cart[i]==0) delete cart[i];
                    $('#budget').text(budget);
                    renderCart();
                })

                let del = $('<a href="#"><i class="fa fa-ban" aria-hidden="true"></i></a>')

                del.on('click', () => {
                    budget += +itemDictionary[i].price * +v;
                    delete cart[i];
                    $('#budget').text(budget);
                    renderCart();
                })

                actioncol.append(add);
                actioncol.append(sub);
                actioncol.append(del);
                tr.append(actioncol)
                
                $('#cart-tbody').append(tr);
            })
        }
    }
    
    $('#saveCust').on('click', () => {
        let custName = $('#custName').val();
        sessionStorage.setItem("custName", custName);
        sessionStorage.setItem("custAvatar", $("#custAvatar").val());
        $('#custGreeting').text("Welcome, "+custName+".");
        $("#login-modal").modal('hide');
    });

    $('#submitCart').on('click', () => {
        if($.isEmptyObject(cart)) {
            $('#cart-modal').modal('close');
            return;
        }

        let toSubmit = {
            "customerType" : sessionStorage.custAvatar,
            "customerName" : sessionStorage.custName,
            "items" : []
        }

        $.each(cart, (i, v) => {
            for(let j=0; j<v; j++) {
                toSubmit['items'].push(i);
            }
        })

        $('#submitCart button').prop('disabled', true);
        console.log(toSubmit);
        $.post(remoteServer, toSubmit).done(() => {
            $('#cart-modal').modal('hide')
            $('#submitCart button').prop('disabled', false);
            budget = 1000;
            let _p = $(`<div class="alert alert-success alert-dismissible fade show" role="alert">
            Order submitted. You may issue another order.
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
            </div>`);

            $('#alert-container').append(_p);
            $('#budget').text(budget);
            setTimeout(() => {
                _p.alert('close')
            }, 3000);
            cart = {};

        }).fail(() => {
            $('#cart-modal').modal('hide')
            $('#submitCart button').prop('disabled', false);
            let _p = $(`<div class="alert alert-danger alert-dismissible fade show" role="alert">
            Order submission failed.
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
            </div>`);

            $('#alert-container').append(_p);
            setTimeout(() => {
                _p.alert('close')
            }, 3000);
        })
    })

    $('#changeCust').on('click', () => {
        $('#login-modal').modal('show');
    })

    $('#showCart').on('click', () => {
        renderCart();
        $('#cart-modal').modal('show')
    })

    if (sessionStorage.custName === undefined) {
        $('#login-modal').modal('show');
        //alert("Hello World!");
    } else {
        $('#custGreeting').text('Welcome, '+sessionStorage.custName+'.');
    }

    for(let i=0; i<itemDictionary.length; ++i) {
        $('#itemlist').append(`
        <a href="#" class="list-group-item list-group-item-action flex-column align-items-start">
            <img src="${itemDictionary[i].image}" class="rounded float-left" style="margin-right:20px;width:80px;height:80px;" />
            <h5 class="mb-1">${itemDictionary[i].title} ($${itemDictionary[i].price})</h5>
            <p class="mb-1">${itemDictionary[i].description}</p>
            <button id="btn-add-${itemDictionary[i].itemId}" class="btn btn-outline-success">Add to cart</button>
        </a>
        `);

        $('#btn-add-'+itemDictionary[i].itemId).on('click', () => {
            if(budget >= itemDictionary[i].price) {
                budget -= itemDictionary[i].price;
                if (i in cart) {
                    cart[i] ++;
                } else {
                    cart[i] = 1;
                }

                let _p = $(`<div class="alert alert-success alert-dismissible fade show" role="alert">
                Item added to cart.
                <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
                </div>`);

                $('#alert-container').append(_p);
                $('#budget').text(budget);
                setTimeout(() => {
                    _p.alert('close')
                }, 3000);
            } else {
                let _p = $(`<div class="alert alert-danger alert-dismissible fade show" role="alert">
                Not enough money.
                <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
                </div>`);

                $('#alert-container').append(_p);
                $('#budget').text(budget);
                setTimeout(() => {
                    _p.alert('close')
                }, 3000);
            }
        });
    }

    
});