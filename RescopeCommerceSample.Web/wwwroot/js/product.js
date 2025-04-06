const inBasket = document.querySelector('.in-basket');
const variantRadios = document.querySelectorAll('.variantRadio');
if (variantRadios.length) {
    variantRadios.forEach(opt => {
        opt.addEventListener('change', (e) => {
            if (opt.checked) {
                if (opt.value && opt.dataset.stock != undefined) {
                    if (inBasket) inBasket.classList.add('hidden');
                    if (opt.dataset.stock == 'dis') {
                        document.querySelector('.addtobasket').classList.remove('hidden');
                    }
                    else { 
                        if (parseInt(opt.dataset.stock) > 0) {
                            document.querySelector('.addtobasket').classList.remove('hidden');
                        }
                        else {
                            document.querySelector('.addtobasket').classList.add('hidden');
                        }
                    }
                }
                else {
                    document.querySelector('.addtobasket').classList.add('hidden'); 
                    if (inBasket) inBasket.classList.remove('hidden');
                }
            }
        })
    });
}