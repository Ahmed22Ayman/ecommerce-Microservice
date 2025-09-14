package com.konecta.product_service.messaging;

import org.springframework.amqp.core.Binding;
import org.springframework.amqp.core.BindingBuilder;
import org.springframework.amqp.core.Queue;
import org.springframework.amqp.core.TopicExchange;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OrderEventsConfig {
    public static final String EXCHANGE = "order.events";
    public static final String ORDER_CREATED_ROUTING_KEY = "order.created";
    public static final String ORDER_CREATED_QUEUE = "order.created.queue";

    @Bean
    public TopicExchange orderEventsExchange() {
        return new TopicExchange(EXCHANGE, true, false);
    }

    @Bean
    public Queue orderCreatedQueue() {
        return new Queue(ORDER_CREATED_QUEUE, true);
    }

    @Bean
    public Binding orderCreatedBinding(Queue orderCreatedQueue, TopicExchange orderEventsExchange) {
        return BindingBuilder.bind(orderCreatedQueue).to(orderEventsExchange).with(ORDER_CREATED_ROUTING_KEY);
    }
}
