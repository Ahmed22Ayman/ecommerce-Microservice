package com.konecta.order_service.messaging;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.konecta.order_service.model.Order;
import com.konecta.order_service.repository.OrderRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
@Slf4j
public class PaymentEventsListener {

    private final OrderRepository orderRepository;
    private final ObjectMapper objectMapper = new ObjectMapper();

    @RabbitListener(queues = PaymentEventsConfig.PAYMENT_SUCCESS_QUEUE)
    public void onPaymentSuccess(byte[] body) {
        handlePaymentEvent(body, true);
    }

    @RabbitListener(queues = PaymentEventsConfig.PAYMENT_FAILED_QUEUE)
    public void onPaymentFailed(byte[] body) {
        handlePaymentEvent(body, false);
    }

    private void handlePaymentEvent(byte[] body, boolean success) {
        try {
            JsonNode node = objectMapper.readTree(body);
            long orderId = node.get("orderId").asLong();
            String status = success ? "PAID" : "CANCELLED";

            Order order = orderRepository.findById(orderId)
                    .orElse(null);
            if (order == null) {
                log.warn("Order not found for payment event, orderId={}", orderId);
                return;
            }
            order.setStatus(status);
            orderRepository.save(order);
            log.info("Updated order {} status to {} due to payment {}", orderId, status, success ? "success" : "failure");
        } catch (Exception e) {
            log.error("Failed to process payment event", e);
        }
    }
}
