package com.konecta.product_service.messaging;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;

@Component
public class OrderCreatedListener {
    private static final Logger log = LoggerFactory.getLogger(OrderCreatedListener.class);

    @RabbitListener(queues = OrderEventsConfig.ORDER_CREATED_QUEUE)
    public void onOrderCreated(Object message) {
        // TODO: map the message to a DTO and reserve stock
        log.info("[ProductService] Received OrderCreated event: {}", message);
    }
}
