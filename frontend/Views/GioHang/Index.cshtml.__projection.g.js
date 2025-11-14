/* BEGIN EXTERNAL SOURCE */

    $(document).ready(function () {

        // --- Handle UPDATE (Increase/Decrease) ---
        $('#cart-item-list').on('click', '.cart-update-btn', function (e) {
            e.preventDefault();

            var $button = $(this);
            var productId = $button.data('product-id');
            var operation = $button.data('operation');

            var $quantitySpan = $('#quantity-' + productId);
            var currentQuantity = parseInt($quantitySpan.text());

            var newQuantity = (operation === 'increase') ? currentQuantity + 1 : currentQuantity - 1;

            $button.closest('.cart-product').css('opacity', 0.5);

            $.post('/**********************************/',
                   { productId: productId, newQuantity: newQuantity },
                   function (response) {

                $('#product-row-' + productId).css('opacity', 1);

                if (response.success) { // <-- Handle success

                    if (response.itemQuantity <= 0) {
                        $('#product-row-' + productId).fadeOut(300, function() {
                            $(this).remove();
                            if ($('#cart-item-list .cart-product').length === 0) {
                                location.reload();
                            }
                        });
                    } else {
                        $quantitySpan.text(response.itemQuantity);
                    }

                    $('#summary-subtotal').text(response.totalAmount);
                    $('#summary-total').text(response.totalAmount);
                    $('.header-cart-link').text('Giỏ hàng (' + response.cartItemCount + ')');

                    if (response.message) {
                        $('#cart-error-message').text(response.message).show();
                    } else {
                        $('#cart-error-message').hide();
                    }
                }
                else { // <-- [NEW] Handle failure
                    $('#cart-error-message').text(response.message).show();
                }
            });
        });

        // --- Handle REMOVE ---
        $('#cart-item-list').on('submit', '.cart-remove-form', function (e) {
            e.preventDefault();

            var $form = $(this);
            var url = $form.attr('action');
            var data = $form.serialize();

            if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
                $.post(url, data, function (response) {
                    if (response.success) {
                        $form.closest('.cart-product').fadeOut(300, function () {
                            $(this).remove();
                            if ($('#cart-item-list .cart-product').length === 0) {
                                location.reload();
                            }
                        });

                        $('#summary-subtotal').text(response.totalAmount);
                        $('#summary-total').text(response.totalAmount);
                        $('.header-cart-link').text('Giỏ hàng (' + response.cartItemCount + ')');
                    }
                });
            }
        });

        // --- Handle DISCOUNT ---
        $('#discount-form').on('submit', function (e) {
            e.preventDefault();

            var $form = $(this);
            var $messageDiv = $('#discount-message');
            var data = $form.serialize();
            var url = $form.attr('action');

            $.post(url, data, function (response) {
                if (response.success) {
                    $('#summary-discount').text(response.discountAmountFormatted);
                    $('#summary-total').text(response.finalTotalFormatted);
                    $('#discount-line').show(); // Ensure this ID exists

                    $messageDiv.text(response.message)
                               .css({ 'background-color': '#e6f7ec', 'border': '1px solid #00b33c', 'color': '#00521b' })
                               .show();
                } else {
                    $('#discount-line').hide();
                    $('#summary-discount').text("0 đ");
                    $('#summary-total').text($('#summary-subtotal').text());

                    $messageDiv.text(response.message)
                               .css({ 'background-color': '#ffebe6', 'border': '1px solid #e43f3f', 'color': '#b71c1c' })
                               .show();
                }
            });
        });
    });
    
/* END EXTERNAL SOURCE */
