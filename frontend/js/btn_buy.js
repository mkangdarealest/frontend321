document.addEventListener("DOMContentLoaded", function (e) {
    function getProductInfo(card) {
        const nameElement = card.querySelector('h3') || card.querySelector('h1');
        const priceElement = card.querySelector('.price .current') || card.querySelector('.price ');

        return {
            name: nameElement?.innerText || '',
            price: priceElement?.innerText || '',
            image: card.querySelector('img')?.getAttribute('src') || '',
            quantity: 1
        };
    }

    function addToCart(product) {
        let cart = JSON.parse(localStorage.getItem('cart')) || [];
        let found = cart.find(item => item.name === product.name);
        if (found) {
            found.quantity += 1;
        } else {
            cart.push(product);
        }
        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartCount();
    }

    function updateCartCount() {
        let cart = JSON.parse(localStorage.getItem('cart')) || [];
        let total = cart.reduce((sum, item) => sum + item.quantity, 0);
        let cartCount = document.getElementById('cart-count');
        if (cartCount) cartCount.innerText = total;
    }

    document.querySelectorAll('.btn-buy, .btn-order-now,.buy-button ').forEach(btn => {
        btn.addEventListener('click', function () {
            const card = btn.closest('.lc-product-card,.product-container');
            if (card) {
                const product = getProductInfo(card);
                addToCart(product);
                alert('Sản phẩm đã được thêm vào giỏ!');
            }
        });
    });
    updateCartCount();
});